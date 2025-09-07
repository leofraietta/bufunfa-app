using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço responsável pelo gerenciamento automático de folhas mensais
    /// Implementa a criação automática de folhas e projeções futuras
    /// Segue os princípios SOLID e Single Responsibility
    /// </summary>
    public class FolhaAutomaticaService : IFolhaAutomaticaService
    {
        private readonly ApplicationDbContext _context;
        private readonly FolhaMensalService _folhaMensalService;

        public FolhaAutomaticaService(ApplicationDbContext context, FolhaMensalService folhaMensalService)
        {
            _context = context;
            _folhaMensalService = folhaMensalService;
        }

        /// <summary>
        /// Cria automaticamente a folha do mês atual quando uma nova conta é criada
        /// REGRA: Ao criar uma conta, automaticamente a folha do mês atual é criada
        /// </summary>
        public async Task CriarFolhaMensalInicialAsync(Conta conta, int usuarioId)
        {
            var agora = DateTime.Now;
            var ano = agora.Year;
            var mes = agora.Month;

            // Verifica se já existe folha para este mês
            var folhaExistente = await _context.FolhasMensais
                .Include(f => f.Conta)
                    .ThenInclude(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(f => f.ContaId == conta.Id && 
                                         f.Ano == ano && 
                                         f.Mes == mes &&
                                         f.Conta.ContaUsuarios.Any(cu => cu.UsuarioId == usuarioId && cu.Ativo));

            if (folhaExistente == null)
            {
                await _folhaMensalService.AbrirFolhaMensalAsync(usuarioId, conta.Id, ano, mes);
            }
        }

        /// <summary>
        /// Cria folhas futuras para projeção financeira
        /// REGRA: Usuário pode criar folhas de meses futuros para visualizar projeções
        /// </summary>
        public async Task<FolhaMensal> CriarFolhaFuturaAsync(int usuarioId, int contaId, int ano, int mes)
        {
            // Validar se é uma data futura válida
            var dataFolha = new DateTime(ano, mes, 1);
            var agora = DateTime.Now;
            var mesAtual = new DateTime(agora.Year, agora.Month, 1);

            if (dataFolha < mesAtual)
            {
                throw new ArgumentException("Não é possível criar folhas para meses passados");
            }

            // Criar ou obter a folha
            var folha = await _folhaMensalService.AbrirFolhaMensalAsync(usuarioId, contaId, ano, mes);

            return folha;
        }

        /// <summary>
        /// Processa folhas futuras automáticas para lançamentos recorrentes/parcelados
        /// REGRA: Quando uma folha futura é aberta, todos os lançamentos aplicáveis devem ser processados
        /// </summary>
        public async Task ProcessarFolhasFuturasAutomaticasAsync(int usuarioId, int contaId, int mesesAFrente = 12)
        {
            var dataAtual = DateTime.Now;
            var mesInicial = new DateTime(dataAtual.Year, dataAtual.Month, 1);

            // Buscar lançamentos ativos que podem gerar folhas futuras
            var lancamentosAtivos = await _context.Lancamentos
                .Where(l => l.UsuarioId == usuarioId && 
                           l.ContaId == contaId && 
                           l.Ativo &&
                           (l.TipoRecorrencia == TipoRecorrencia.Recorrente ||
                            l.TipoRecorrencia == TipoRecorrencia.Parcelado))
                .ToListAsync();

            for (int i = 1; i <= mesesAFrente; i++)
            {
                var mesFuturo = mesInicial.AddMonths(i);
                
                // Verificar se algum lançamento ativo deve aparecer neste mês
                var temLancamentoNoMes = lancamentosAtivos.Any(l => 
                    DeveProcessarLancamentoNoMes(l, mesFuturo));

                if (temLancamentoNoMes)
                {
                    // Criar folha se não existir
                    await _folhaMensalService.AbrirFolhaMensalAsync(
                        usuarioId, contaId, mesFuturo.Year, mesFuturo.Month);
                }
            }
        }

        /// <summary>
        /// Atualiza todas as folhas futuras quando um lançamento é alterado
        /// REGRA: Alterações em lançamentos recorrentes/parcelados devem refletir em folhas futuras
        /// </summary>
        public async Task AtualizarFolhasFuturasAsync(Lancamento lancamento)
        {
            var agora = DateTime.Now;
            var mesAtual = new DateTime(agora.Year, agora.Month, 1);

            // Buscar todas as folhas futuras desta conta
            var folhasFuturas = await _context.FolhasMensais
                .Include(f => f.Conta)
                    .ThenInclude(c => c.ContaUsuarios)
                .Where(f => f.ContaId == lancamento.ContaId &&
                           new DateTime(f.Ano, f.Mes, 1) > mesAtual &&
                           f.Conta.ContaUsuarios.Any(cu => cu.UsuarioId == lancamento.UsuarioId && cu.Ativo))
                .ToListAsync();

            foreach (var folha in folhasFuturas)
            {
                // Remover lançamentos existentes deste lançamento origem que não foram realizados
                var lancamentosParaRemover = await _context.LancamentosFolha
                    .Where(lf => lf.FolhaMensalId == folha.Id && 
                                lf.LancamentoOrigemId == lancamento.Id && 
                                !lf.Realizado)
                    .ToListAsync();

                _context.LancamentosFolha.RemoveRange(lancamentosParaRemover);

                // Reprocessar o lançamento para esta folha se ainda for aplicável
                if (DeveProcessarLancamentoNoMes(lancamento, new DateTime(folha.Ano, folha.Mes, 1)))
                {
                    await _folhaMensalService.ReprocessarLancamentosFolhaAsync(folha.Id);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica se um lançamento deve ser processado em um determinado mês
        /// </summary>
        private bool DeveProcessarLancamentoNoMes(Lancamento lancamento, DateTime mesFolha)
        {
            var dataInicioMes = new DateTime(mesFolha.Year, mesFolha.Month, 1);
            var dataFimMes = dataInicioMes.AddMonths(1).AddDays(-1);

            // Verificar se o lançamento está ativo neste período
            if (lancamento.DataInicial.Date > dataFimMes)
                return false;

            if (lancamento.DataFinal.HasValue && lancamento.DataFinal.Value.Date < dataInicioMes)
                return false;

            // Usar polimorfismo para verificar se pode ser processado
            var datasVencimento = lancamento.ObterDatasVencimento(dataInicioMes, dataFimMes);
            return datasVencimento.Any();
        }

        /// <summary>
        /// Cria folhas automáticas para os próximos N meses baseado nos lançamentos ativos
        /// Útil para dashboard de projeções
        /// </summary>
        public async Task CriarFolhasProjecaoAsync(int usuarioId, int contaId, int mesesProjecao = 6)
        {
            await ProcessarFolhasFuturasAutomaticasAsync(usuarioId, contaId, mesesProjecao);
        }

        /// <summary>
        /// Remove folhas futuras vazias (sem lançamentos)
        /// Útil para limpeza de folhas desnecessárias
        /// </summary>
        public async Task RemoverFolhasVaziasAsync(int usuarioId, int contaId)
        {
            var agora = DateTime.Now;
            var mesAtual = new DateTime(agora.Year, agora.Month, 1);

            var folhasVazias = await _context.FolhasMensais
                .Include(f => f.LancamentosFolha)
                .Include(f => f.Conta)
                    .ThenInclude(c => c.ContaUsuarios)
                .Where(f => f.ContaId == contaId &&
                           new DateTime(f.Ano, f.Mes, 1) > mesAtual &&
                           !f.LancamentosFolha.Any() &&
                           !f.Fechada &&
                           f.Conta.ContaUsuarios.Any(cu => cu.UsuarioId == usuarioId && cu.Ativo))
                .ToListAsync();

            _context.FolhasMensais.RemoveRange(folhasVazias);
            await _context.SaveChangesAsync();
        }
    }
}
