using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    public class FolhaMensalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILancamentoProcessorService _lancamentoProcessor;

        public FolhaMensalService(ApplicationDbContext context, ILancamentoProcessorService lancamentoProcessor)
        {
            _context = context;
            _lancamentoProcessor = lancamentoProcessor;
        }

        public async Task<FolhaMensal> AbrirFolhaMensalAsync(int usuarioId, int contaId, int ano, int mes)
        {
            // Verificar se a folha já existe
            var folhaExistente = await _context.FolhasMensais
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.ContaId == contaId && f.Ano == ano && f.Mes == mes);

            if (folhaExistente != null)
            {
                return folhaExistente;
            }

            // Obter saldo inicial (saldo final do mês anterior)
            var saldoInicial = await ObterSaldoInicialAsync(usuarioId, contaId, ano, mes);

            // Criar nova folha mensal
            var novaFolha = new FolhaMensal
            {
                UsuarioId = usuarioId,
                ContaId = contaId,
                Ano = ano,
                Mes = mes,
                SaldoInicialReal = saldoInicial.Real,
                SaldoInicialProvisionado = saldoInicial.Provisionado,
                DataCriacao = DateTime.UtcNow
            };

            _context.FolhasMensais.Add(novaFolha);
            await _context.SaveChangesAsync();

            // Propagar lançamentos para a folha usando o serviço especializado
            await PropagrarLancamentosParaFolhaAsync(novaFolha);

            // Calcular saldos finais
            await CalcularSaldosFinaisAsync(novaFolha.Id);

            return novaFolha;
        }

        public async Task<FolhaMensal> ObterFolhaMensalAsync(int usuarioId, int contaId, int ano, int mes)
        {
            var folha = await _context.FolhasMensais
                .Include(f => f.LancamentosFolha)
                    .ThenInclude(lf => lf.Categoria)
                .Include(f => f.Conta)
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.ContaId == contaId && f.Ano == ano && f.Mes == mes);

            if (folha == null)
            {
                // Criar automaticamente se não existir
                folha = await AbrirFolhaMensalAsync(usuarioId, contaId, ano, mes);
            }

            return folha;
        }

        public async Task<List<FolhaMensal>> ObterFolhasMensaisUsuarioAsync(int usuarioId, int ano, int mes)
        {
            return await _context.FolhasMensais
                .Include(f => f.Conta)
                .Include(f => f.LancamentosFolha)
                .Where(f => f.UsuarioId == usuarioId && f.Ano == ano && f.Mes == mes)
                .ToListAsync();
        }

        private async Task<(decimal Real, decimal Provisionado)> ObterSaldoInicialAsync(int usuarioId, int contaId, int ano, int mes)
        {
            // Para o primeiro mês, usar saldo inicial da conta
            var conta = await _context.Contas.FindAsync(contaId);
            if (conta == null)
                return (0, 0);

            // Verificar se há folha do mês anterior
            var (anoAnterior, mesAnterior) = ObterMesAnterior(ano, mes);
            var folhaAnterior = await _context.FolhasMensais
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.ContaId == contaId && f.Ano == anoAnterior && f.Mes == mesAnterior);

            if (folhaAnterior != null)
            {
                return (folhaAnterior.SaldoFinalReal, folhaAnterior.SaldoFinalProvisionado);
            }

            // Se não há folha anterior, usar saldo inicial da conta
            return (conta.SaldoInicial, conta.SaldoInicial);
        }

        private async Task PropagrarLancamentosParaFolhaAsync(FolhaMensal folha)
        {
            // Usar o serviço especializado para processar lançamentos
            var lancamentosFolha = await _lancamentoProcessor.ProcessarLancamentosParaFolhaAsync(
                folha.UsuarioId, folha.ContaId, folha);

            // Adicionar os lançamentos gerados ao contexto
            foreach (var lancamentoFolha in lancamentosFolha)
            {
                _context.LancamentosFolha.Add(lancamentoFolha);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Reprocessa lançamentos para uma folha específica
        /// Útil quando há alterações nos lançamentos origem
        /// </summary>
        public async Task ReprocessarLancamentosFolhaAsync(int folhaMensalId)
        {
            var folha = await _context.FolhasMensais
                .Include(f => f.LancamentosFolha)
                .FirstOrDefaultAsync(f => f.Id == folhaMensalId);

            if (folha == null) return;

            // Remove lançamentos existentes que não foram realizados
            var lancamentosParaRemover = folha.LancamentosFolha
                .Where(lf => !lf.Realizado)
                .ToList();

            _context.LancamentosFolha.RemoveRange(lancamentosParaRemover);
            await _context.SaveChangesAsync();

            // Reprocessa os lançamentos
            await PropagrarLancamentosParaFolhaAsync(folha);

            // Recalcula saldos
            await CalcularSaldosFinaisAsync(folha.Id);
        }

        /// <summary>
        /// Adiciona um novo lançamento a uma folha existente
        /// Usado quando um lançamento esporádico é criado diretamente na folha
        /// </summary>
        public async Task<LancamentoFolha> AdicionarLancamentoEsporadicoAsync(int folhaMensalId, Lancamento lancamentoEsporadico)
        {
            var folha = await _context.FolhasMensais.FindAsync(folhaMensalId);
            if (folha == null) return null;

            // Verifica se é realmente esporádico e se a data corresponde ao mês da folha
            if (lancamentoEsporadico.TipoRecorrencia != TipoRecorrencia.Esporadico)
                throw new ArgumentException("Apenas lançamentos esporádicos podem ser adicionados diretamente à folha");

            var dataInicioFolha = new DateTime(folha.Ano, folha.Mes, 1);
            var dataFimFolha = dataInicioFolha.AddMonths(1).AddDays(-1);

            if (lancamentoEsporadico.DataInicial.Date < dataInicioFolha || lancamentoEsporadico.DataInicial.Date > dataFimFolha)
                throw new ArgumentException("A data do lançamento esporádico deve corresponder ao mês da folha");

            // Processa o lançamento para a folha
            var lancamentosFolha = await _lancamentoProcessor.ProcessarLancamentoParaFolhaAsync(lancamentoEsporadico, folha);
            var lancamentoFolha = lancamentosFolha.FirstOrDefault();

            if (lancamentoFolha != null)
            {
                _context.LancamentosFolha.Add(lancamentoFolha);
                await _context.SaveChangesAsync();

                // Recalcula saldos da folha
                await CalcularSaldosFinaisAsync(folha.Id);
            }

            return lancamentoFolha;
        }

        private async Task CalcularSaldosFinaisAsync(int folhaMensalId)
        {
            var folha = await _context.FolhasMensais
                .Include(f => f.LancamentosFolha)
                .FirstOrDefaultAsync(f => f.Id == folhaMensalId);

            if (folha == null) return;

            // Calcular totais de receitas e despesas
            var receitasReais = folha.LancamentosFolha
                .Where(lf => lf.Tipo == TipoLancamento.Receita && lf.Realizado)
                .Sum(lf => lf.ValorReal ?? 0);

            var receitasProvisionadas = folha.LancamentosFolha
                .Where(lf => lf.Tipo == TipoLancamento.Receita)
                .Sum(lf => lf.ValorProvisionado);

            var despesasReais = folha.LancamentosFolha
                .Where(lf => lf.Tipo == TipoLancamento.Despesa && lf.Realizado)
                .Sum(lf => lf.ValorReal ?? 0);

            var despesasProvisionadas = folha.LancamentosFolha
                .Where(lf => lf.Tipo == TipoLancamento.Despesa)
                .Sum(lf => lf.ValorProvisionado);

            // Atualizar folha
            folha.TotalReceitasReais = receitasReais;
            folha.TotalReceitasProvisionadas = receitasProvisionadas;
            folha.TotalDespesasReais = despesasReais;
            folha.TotalDespesasProvisionadas = despesasProvisionadas;

            folha.SaldoFinalReal = folha.SaldoInicialReal + receitasReais - despesasReais;
            folha.SaldoFinalProvisionado = folha.SaldoInicialProvisionado + receitasProvisionadas - despesasProvisionadas;

            await _context.SaveChangesAsync();
        }

        public async Task AtualizarLancamentoFolhaAsync(int lancamentoFolhaId, decimal? valorReal, DateTime? dataRealizacao)
        {
            var lancamentoFolha = await _context.LancamentosFolha.FindAsync(lancamentoFolhaId);
            if (lancamentoFolha == null) return;

            lancamentoFolha.ValorReal = valorReal;
            lancamentoFolha.DataRealizacao = dataRealizacao;
            lancamentoFolha.Realizado = valorReal.HasValue;
            lancamentoFolha.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recalcular saldos da folha
            await CalcularSaldosFinaisAsync(lancamentoFolha.FolhaMensalId);
        }

        private (int Ano, int Mes) ObterMesAnterior(int ano, int mes)
        {
            if (mes == 1)
            {
                return (ano - 1, 12);
            }
            return (ano, mes - 1);
        }

        private (int Ano, int Mes) ObterProximoMes(int ano, int mes)
        {
            if (mes == 12)
            {
                return (ano + 1, 1);
            }
            return (ano, mes + 1);
        }
    }
}

