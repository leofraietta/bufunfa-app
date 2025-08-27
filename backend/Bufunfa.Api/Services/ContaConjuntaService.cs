using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.Factories;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço para gestão de contas conjuntas
    /// Implementa lógicas específicas de rateio, apuração e distribuição
    /// </summary>
    public class ContaConjuntaService : IContaConjuntaService
    {
        private readonly ApplicationDbContext _context;
        private readonly FolhaMensalService _folhaMensalService;
        private readonly ILancamentoFactory _lancamentoFactory;

        public ContaConjuntaService(
            ApplicationDbContext context, 
            FolhaMensalService folhaMensalService,
            ILancamentoFactory lancamentoFactory)
        {
            _context = context;
            _folhaMensalService = folhaMensalService;
            _lancamentoFactory = lancamentoFactory;
        }

        /// <summary>
        /// Processa a apuração mensal da conta conjunta e distribui rateios
        /// </summary>
        public async Task<ApuracaoResult> ProcessarApuracaoMensalAsync(int contaConjuntaId, int ano, int mes)
        {
            var contaConjunta = await _context.Contas
                .Include(c => c.ContaUsuarios.Where(cu => cu.Ativo))
                .ThenInclude(cu => cu.Usuario)
                .OfType<ContaConjunta>()
                .FirstOrDefaultAsync(c => c.Id == contaConjuntaId);

            if (contaConjunta == null)
                throw new ArgumentException("Conta conjunta não encontrada");

            // Obter folha da conta conjunta para o mês
            var folhaConjunta = await _folhaMensalService.ObterFolhaMensalAsync(
                contaConjunta.UsuarioCriadorId.Value, contaConjuntaId, ano, mes);

            // Processar apuração
            var apuracao = contaConjunta.ProcessarApuracao(ano, mes);
            apuracao.SaldoApurado = folhaConjunta.SaldoFinalProvisionado; // Usar saldo provisionado da folha

            // Distribuir rateio nas folhas dos usuários
            await DistribuirRateioNasFolhasAsync(contaConjunta, apuracao);

            // Salvar alterações
            await _context.SaveChangesAsync();

            return apuracao;
        }

        /// <summary>
        /// Distribui o rateio da conta conjunta nas folhas individuais dos usuários
        /// REGRA: Saldo rateado aparece na folha do usuário na data de fechamento configurada
        /// </summary>
        public async Task DistribuirRateioNasFolhasAsync(ContaConjunta contaConjunta, ApuracaoResult apuracao)
        {
            foreach (var rateio in apuracao.Rateios)
            {
                var usuarioId = rateio.Key;
                var valorRateado = rateio.Value;

                // Buscar conta principal do usuário
                var contaPrincipal = await _context.Contas
                    .Include(c => c.ContaUsuarios)
                    .Where(c => c.ContaUsuarios.Any(cu => cu.UsuarioId == usuarioId && cu.EhProprietario && cu.Ativo))
                    .OfType<ContaCorrente>()
                    .FirstOrDefaultAsync();

                if (contaPrincipal == null) continue;

                // Obter ou criar folha do usuário para o mês
                var folhaUsuario = await _folhaMensalService.ObterFolhaMensalAsync(
                    usuarioId, contaPrincipal.Id, apuracao.Ano, apuracao.Mes);

                // Criar lançamento de rateio na folha do usuário
                await CriarLancamentoRateioAsync(contaConjunta, folhaUsuario, valorRateado, apuracao.DataApuracao);
            }
        }

        /// <summary>
        /// Cria lançamento de rateio na folha do usuário
        /// </summary>
        private async Task CriarLancamentoRateioAsync(ContaConjunta contaConjunta, FolhaMensal folhaUsuario, 
            decimal valorRateado, DateTime dataApuracao)
        {
            // Verificar se já existe lançamento de rateio para este mês
            var lancamentoExistente = await _context.LancamentosFolha
                .FirstOrDefaultAsync(lf => lf.FolhaMensalId == folhaUsuario.Id && 
                                          lf.Descricao.Contains($"Rateio - {contaConjunta.Nome}"));

            if (lancamentoExistente != null)
            {
                // Atualizar valor existente
                lancamentoExistente.ValorProvisionado = Math.Abs(valorRateado);
                lancamentoExistente.Tipo = valorRateado >= 0 ? TipoLancamento.Receita : TipoLancamento.Despesa;
                lancamentoExistente.DataAtualizacao = DateTime.UtcNow;
            }
            else
            {
                // Criar novo lançamento
                var lancamentoRateio = new LancamentoFolha
                {
                    FolhaMensalId = folhaUsuario.Id,
                    LancamentoOrigemId = 0, // Não tem lançamento origem específico
                    Descricao = $"Rateio - {contaConjunta.Nome}",
                    ValorProvisionado = Math.Abs(valorRateado),
                    DataPrevista = dataApuracao,
                    Tipo = valorRateado >= 0 ? TipoLancamento.Receita : TipoLancamento.Despesa,
                    TipoRecorrencia = TipoRecorrencia.Esporadico,
                    Realizado = false,
                    DataCriacao = DateTime.UtcNow
                };

                _context.LancamentosFolha.Add(lancamentoRateio);
            }

            // Recalcular saldos da folha
            await _folhaMensalService.CalcularSaldosFinaisAsync(folhaUsuario.Id);
        }

        /// <summary>
        /// Verifica se um usuário tem permissão para uma operação específica na conta
        /// </summary>
        public bool UsuarioTemPermissao(int usuarioId, int contaId, TipoPermissao permissao)
        {
            var contaUsuario = _context.ContaUsuarios
                .FirstOrDefault(cu => cu.UsuarioId == usuarioId && cu.ContaId == contaId && cu.Ativo);

            if (contaUsuario == null) return false;

            return permissao switch
            {
                TipoPermissao.Leitura => contaUsuario.PodeLer,
                TipoPermissao.Escrita => contaUsuario.PodeEscrever,
                TipoPermissao.Administracao => contaUsuario.PodeAdministrar,
                _ => false
            };
        }

        /// <summary>
        /// Adiciona um usuário à conta conjunta com permissões específicas
        /// </summary>
        public async Task<ContaUsuario> AdicionarUsuarioAsync(int contaConjuntaId, int usuarioId, decimal percentualParticipacao,
            bool podeLer = true, bool podeEscrever = false, bool podeAdministrar = false)
        {
            // Verificar se o usuário já está na conta
            var contaUsuarioExistente = await _context.ContaUsuarios
                .FirstOrDefaultAsync(cu => cu.ContaId == contaConjuntaId && cu.UsuarioId == usuarioId);

            if (contaUsuarioExistente != null)
            {
                if (contaUsuarioExistente.Ativo)
                    throw new InvalidOperationException("Usuário já está vinculado à conta");

                // Reativar usuário
                contaUsuarioExistente.Ativo = true;
                contaUsuarioExistente.PercentualParticipacao = percentualParticipacao;
                contaUsuarioExistente.PodeLer = podeLer;
                contaUsuarioExistente.PodeEscrever = podeEscrever;
                contaUsuarioExistente.PodeAdministrar = podeAdministrar;
                contaUsuarioExistente.DataVinculacao = DateTime.UtcNow;
                contaUsuarioExistente.DataDesvinculacao = null;

                await _context.SaveChangesAsync();
                return contaUsuarioExistente;
            }

            // Criar nova vinculação
            var novaVinculacao = new ContaUsuario
            {
                ContaId = contaConjuntaId,
                UsuarioId = usuarioId,
                PercentualParticipacao = percentualParticipacao,
                PodeLer = podeLer,
                PodeEscrever = podeEscrever,
                PodeAdministrar = podeAdministrar,
                EhProprietario = false,
                Ativo = true,
                DataVinculacao = DateTime.UtcNow
            };

            _context.ContaUsuarios.Add(novaVinculacao);
            await _context.SaveChangesAsync();

            return novaVinculacao;
        }

        /// <summary>
        /// Remove um usuário da conta conjunta
        /// </summary>
        public async Task RemoverUsuarioAsync(int contaConjuntaId, int usuarioId)
        {
            var contaUsuario = await _context.ContaUsuarios
                .FirstOrDefaultAsync(cu => cu.ContaId == contaConjuntaId && cu.UsuarioId == usuarioId && cu.Ativo);

            if (contaUsuario == null)
                throw new ArgumentException("Usuário não encontrado na conta");

            if (contaUsuario.EhProprietario)
                throw new InvalidOperationException("Não é possível remover o proprietário da conta");

            contaUsuario.Ativo = false;
            contaUsuario.DataDesvinculacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza as permissões de um usuário na conta conjunta
        /// </summary>
        public async Task AtualizarPermissoesUsuarioAsync(int contaConjuntaId, int usuarioId,
            bool? podeLer = null, bool? podeEscrever = null, bool? podeAdministrar = null)
        {
            var contaUsuario = await _context.ContaUsuarios
                .FirstOrDefaultAsync(cu => cu.ContaId == contaConjuntaId && cu.UsuarioId == usuarioId && cu.Ativo);

            if (contaUsuario == null)
                throw new ArgumentException("Usuário não encontrado na conta");

            if (podeLer.HasValue) contaUsuario.PodeLer = podeLer.Value;
            if (podeEscrever.HasValue) contaUsuario.PodeEscrever = podeEscrever.Value;
            if (podeAdministrar.HasValue) contaUsuario.PodeAdministrar = podeAdministrar.Value;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza o percentual de participação de um usuário
        /// </summary>
        public async Task AtualizarPercentualParticipacaoAsync(int contaConjuntaId, int usuarioId, decimal novoPercentual)
        {
            var contaUsuario = await _context.ContaUsuarios
                .FirstOrDefaultAsync(cu => cu.ContaId == contaConjuntaId && cu.UsuarioId == usuarioId && cu.Ativo);

            if (contaUsuario == null)
                throw new ArgumentException("Usuário não encontrado na conta");

            contaUsuario.PercentualParticipacao = novoPercentual;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém o histórico de apurações da conta conjunta
        /// </summary>
        public async Task<List<ApuracaoResult>> ObterHistoricoApuracoesAsync(int contaConjuntaId, int? ano = null)
        {
            // Por enquanto, retorna lista vazia - implementação futura pode incluir tabela de histórico
            return new List<ApuracaoResult>();
        }

        /// <summary>
        /// Calcula a projeção de rateio para um determinado mês
        /// </summary>
        public async Task<Dictionary<int, decimal>> CalcularProjecaoRateioAsync(int contaConjuntaId, int ano, int mes)
        {
            var contaConjunta = await _context.Contas
                .Include(c => c.ContaUsuarios.Where(cu => cu.Ativo))
                .OfType<ContaConjunta>()
                .FirstOrDefaultAsync(c => c.Id == contaConjuntaId);

            if (contaConjunta == null)
                return new Dictionary<int, decimal>();

            // Obter folha da conta conjunta para projeção
            var folhaConjunta = await _folhaMensalService.ObterFolhaMensalAsync(
                contaConjunta.UsuarioCriadorId.Value, contaConjuntaId, ano, mes);

            var saldoProjetado = folhaConjunta.SaldoFinalProvisionado;
            var projecaoRateio = new Dictionary<int, decimal>();

            foreach (var participante in contaConjunta.ContaUsuarios.Where(cu => cu.Ativo))
            {
                var valorRateado = saldoProjetado * (participante.PercentualParticipacao / 100);
                projecaoRateio[participante.UsuarioId] = valorRateado;
            }

            return projecaoRateio;
        }
    }
}
