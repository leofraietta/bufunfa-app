using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.Factories;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço para gestão de cartões de crédito compartilhados
    /// Implementa consolidação de faturas e controle de usuários autorizados
    /// </summary>
    public class CartaoCompartilhadoService : ICartaoCompartilhadoService
    {
        private readonly ApplicationDbContext _context;
        private readonly FolhaMensalService _folhaMensalService;
        private readonly ILancamentoFactory _lancamentoFactory;

        public CartaoCompartilhadoService(
            ApplicationDbContext context,
            FolhaMensalService folhaMensalService,
            ILancamentoFactory lancamentoFactory)
        {
            _context = context;
            _folhaMensalService = folhaMensalService;
            _lancamentoFactory = lancamentoFactory;
        }

        /// <summary>
        /// Consolida a fatura do cartão compartilhado na conta responsável
        /// REGRA: Despesas/receitas do cartão são centralizadas em uma única fatura
        /// </summary>
        public async Task<ConsolidacaoFaturaResult> ConsolidarFaturaAsync(int cartaoId, int ano, int mes)
        {
            var cartao = await _context.Contas
                .Include(c => c.ContaUsuarios.Where(cu => cu.Ativo))
                .Include(c => c.Lancamentos.Where(l => l.Ativo))
                .OfType<ContaCartaoCredito>()
                .FirstOrDefaultAsync(c => c.Id == cartaoId);

            if (cartao == null)
                throw new ArgumentException("Cartão não encontrado");

            var dataVencimento = cartao.CalcularDataVencimento(ano, mes);
            var valorTotal = await CalcularValorFaturaConsolidadaAsync(cartaoId, ano, mes);
            var detalhamentoPorUsuario = await ObterDetalhamentoFaturaPorUsuarioAsync(cartaoId, ano, mes);

            var resultado = new ConsolidacaoFaturaResult
            {
                CartaoId = cartaoId,
                Ano = ano,
                Mes = mes,
                DataVencimento = dataVencimento,
                ValorTotal = valorTotal,
                DetalhamentoPorUsuario = detalhamentoPorUsuario
            };

            // Consolidar na conta responsável
            if (cartao.ContaCorrenteResponsavelId.HasValue)
            {
                await ConsolidarNaContaResponsavelAsync(cartao, resultado);
            }
            else
            {
                // Se não há conta responsável específica, consolidar na conta conjunta (se aplicável)
                await ConsolidarNaContaConjuntaAsync(cartao, resultado);
            }

            resultado.Consolidada = true;
            return resultado;
        }

        /// <summary>
        /// Consolida a fatura na conta corrente responsável
        /// </summary>
        private async Task ConsolidarNaContaResponsavelAsync(ContaCartaoCredito cartao, ConsolidacaoFaturaResult resultado)
        {
            if (!cartao.ContaCorrenteResponsavelId.HasValue) return;

            var contaResponsavel = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == cartao.ContaCorrenteResponsavelId.Value);

            if (contaResponsavel == null) return;

            // Obter usuário proprietário da conta responsável
            var proprietario = contaResponsavel.ContaUsuarios.FirstOrDefault(cu => cu.EhProprietario && cu.Ativo);
            if (proprietario == null) return;

            // Obter folha da conta responsável
            var folhaResponsavel = await _folhaMensalService.ObterFolhaMensalAsync(
                proprietario.UsuarioId, contaResponsavel.Id, resultado.Ano, resultado.Mes);

            // Criar lançamento consolidado
            await CriarLancamentoConsolidadoAsync(cartao, folhaResponsavel, resultado);
        }

        /// <summary>
        /// Consolida a fatura na conta conjunta (se o cartão pertencer a uma)
        /// </summary>
        private async Task ConsolidarNaContaConjuntaAsync(ContaCartaoCredito cartao, ConsolidacaoFaturaResult resultado)
        {
            // Verificar se o cartão pertence a uma conta conjunta
            var contaConjunta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .OfType<ContaConjunta>()
                .FirstOrDefaultAsync(c => c.ContaUsuarios.Any(cu => 
                    cartao.ContaUsuarios.Any(ccu => ccu.UsuarioId == cu.UsuarioId && cu.Ativo)));

            if (contaConjunta == null) return;

            var proprietario = contaConjunta.ContaUsuarios.FirstOrDefault(cu => cu.EhProprietario && cu.Ativo);
            if (proprietario == null) return;

            // Obter folha da conta conjunta
            var folhaConjunta = await _folhaMensalService.ObterFolhaMensalAsync(
                proprietario.UsuarioId, contaConjunta.Id, resultado.Ano, resultado.Mes);

            // Criar lançamento consolidado
            await CriarLancamentoConsolidadoAsync(cartao, folhaConjunta, resultado);
        }

        /// <summary>
        /// Cria lançamento consolidado da fatura na folha
        /// </summary>
        private async Task CriarLancamentoConsolidadoAsync(ContaCartaoCredito cartao, FolhaMensal folha, ConsolidacaoFaturaResult resultado)
        {
            // Verificar se já existe lançamento consolidado para este mês
            var lancamentoExistente = await _context.LancamentosFolha
                .FirstOrDefaultAsync(lf => lf.FolhaMensalId == folha.Id && 
                                          lf.Descricao.Contains($"Fatura {cartao.Nome}"));

            if (lancamentoExistente != null)
            {
                // Atualizar valor existente
                lancamentoExistente.ValorProvisionado = resultado.ValorTotal;
                lancamentoExistente.DataPrevista = resultado.DataVencimento;
                lancamentoExistente.DataAtualizacao = DateTime.UtcNow;
                resultado.LancamentoFolhaId = lancamentoExistente.Id;
            }
            else
            {
                // Criar novo lançamento consolidado
                var lancamentoConsolidado = new LancamentoFolha
                {
                    FolhaMensalId = folha.Id,
                    LancamentoOrigemId = 0, // Não tem lançamento origem específico
                    Descricao = $"Fatura {cartao.Nome} - Consolidada",
                    ValorProvisionado = resultado.ValorTotal,
                    DataPrevista = resultado.DataVencimento,
                    Tipo = TipoLancamento.Despesa,
                    TipoRecorrencia = TipoRecorrencia.Esporadico,
                    Realizado = false,
                    DataCriacao = DateTime.UtcNow
                };

                _context.LancamentosFolha.Add(lancamentoConsolidado);
                await _context.SaveChangesAsync();
                resultado.LancamentoFolhaId = lancamentoConsolidado.Id;
            }

            // Recalcular saldos da folha
            await _folhaMensalService.CalcularSaldosFinaisAsync(folha.Id);
        }

        /// <summary>
        /// Verifica se um usuário pode usar o cartão compartilhado
        /// REGRA: Usuário deve estar vinculado à conta do cartão
        /// </summary>
        public bool UsuarioPodeUsarCartao(int usuarioId, int cartaoId)
        {
            return _context.ContaUsuarios
                .Any(cu => cu.ContaId == cartaoId && cu.UsuarioId == usuarioId && cu.Ativo);
        }

        /// <summary>
        /// Adiciona um usuário autorizado ao cartão compartilhado
        /// </summary>
        public async Task AdicionarUsuarioAutorizadoAsync(int cartaoId, int usuarioId)
        {
            var cartao = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == cartaoId);

            if (cartao == null)
                throw new ArgumentException("Cartão não encontrado");

            // Verificar se o usuário já está autorizado
            var usuarioExistente = cartao.ContaUsuarios
                .FirstOrDefault(cu => cu.UsuarioId == usuarioId);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.Ativo)
                    throw new InvalidOperationException("Usuário já está autorizado no cartão");

                // Reativar usuário
                usuarioExistente.Ativo = true;
                usuarioExistente.DataVinculacao = DateTime.UtcNow;
                usuarioExistente.DataDesvinculacao = null;
            }
            else
            {
                // Adicionar novo usuário autorizado
                var novaAutorizacao = new ContaUsuario
                {
                    ContaId = cartaoId,
                    UsuarioId = usuarioId,
                    PercentualParticipacao = 0, // Cartões não usam percentual
                    PodeLer = true,
                    PodeEscrever = true,
                    PodeAdministrar = false,
                    EhProprietario = false,
                    Ativo = true,
                    DataVinculacao = DateTime.UtcNow
                };

                _context.ContaUsuarios.Add(novaAutorizacao);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove um usuário autorizado do cartão compartilhado
        /// </summary>
        public async Task RemoverUsuarioAutorizadoAsync(int cartaoId, int usuarioId)
        {
            var contaUsuario = await _context.ContaUsuarios
                .FirstOrDefaultAsync(cu => cu.ContaId == cartaoId && cu.UsuarioId == usuarioId && cu.Ativo);

            if (contaUsuario == null)
                throw new ArgumentException("Usuário não está autorizado no cartão");

            if (contaUsuario.EhProprietario)
                throw new InvalidOperationException("Não é possível remover o proprietário do cartão");

            contaUsuario.Ativo = false;
            contaUsuario.DataDesvinculacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém todos os usuários autorizados de um cartão
        /// </summary>
        public async Task<List<Usuario>> ObterUsuariosAutorizadosAsync(int cartaoId)
        {
            return await _context.ContaUsuarios
                .Include(cu => cu.Usuario)
                .Where(cu => cu.ContaId == cartaoId && cu.Ativo)
                .Select(cu => cu.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula o valor da fatura consolidada para um período
        /// </summary>
        public async Task<decimal> CalcularValorFaturaConsolidadaAsync(int cartaoId, int ano, int mes)
        {
            var cartao = await _context.Contas
                .Include(c => c.Lancamentos)
                .OfType<ContaCartaoCredito>()
                .FirstOrDefaultAsync(c => c.Id == cartaoId);

            if (cartao == null) return 0;

            return cartao.CalcularValorFatura(ano, mes);
        }

        /// <summary>
        /// Obtém o detalhamento da fatura por usuário
        /// </summary>
        public async Task<Dictionary<int, decimal>> ObterDetalhamentoFaturaPorUsuarioAsync(int cartaoId, int ano, int mes)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            var lancamentosPorUsuario = await _context.Lancamentos
                .Where(l => l.ContaId == cartaoId && 
                           l.DataInicial >= dataInicio && 
                           l.DataInicial <= dataFim &&
                           l.Ativo)
                .GroupBy(l => l.UsuarioId)
                .Select(g => new { UsuarioId = g.Key, Total = g.Sum(l => l.Valor) })
                .ToDictionaryAsync(x => x.UsuarioId, x => x.Total);

            return lancamentosPorUsuario;
        }
    }
}
