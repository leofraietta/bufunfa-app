using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    public class FolhaMensalService
    {
        private readonly ApplicationDbContext _context;

        public FolhaMensalService(ApplicationDbContext context)
        {
            _context = context;
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

            // Propagar lançamentos para a folha
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
            var dataInicio = new DateTime(folha.Ano, folha.Mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            // Buscar lançamentos que devem aparecer nesta folha
            var lancamentos = await _context.Lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.UsuarioId == folha.UsuarioId && l.ContaId == folha.ContaId && l.Ativo)
                .ToListAsync();

            foreach (var lancamento in lancamentos)
            {
                var lancamentosFolha = GerarLancamentosFolha(lancamento, folha, dataInicio, dataFim);
                foreach (var lancamentoFolha in lancamentosFolha)
                {
                    _context.LancamentosFolha.Add(lancamentoFolha);
                }
            }

            await _context.SaveChangesAsync();
        }

        private List<LancamentoFolha> GerarLancamentosFolha(Lancamento lancamento, FolhaMensal folha, DateTime dataInicio, DateTime dataFim)
        {
            var lancamentosFolha = new List<LancamentoFolha>();

            switch (lancamento.TipoRecorrencia)
            {
                case TipoRecorrencia.Esporadico:
                    // Só adiciona se a data inicial estiver no mês da folha
                    if (lancamento.DataInicial >= dataInicio && lancamento.DataInicial <= dataFim)
                    {
                        lancamentosFolha.Add(CriarLancamentoFolha(lancamento, folha, lancamento.DataInicial, 1, 1));
                    }
                    break;

                case TipoRecorrencia.Recorrente:
                    // Adiciona se o lançamento já estava ativo antes ou durante este mês
                    if (lancamento.DataInicial <= dataFim && (lancamento.DataFinal == null || lancamento.DataFinal >= dataInicio))
                    {
                        var dataVencimento = new DateTime(folha.Ano, folha.Mes, lancamento.DiaVencimento ?? lancamento.DataInicial.Day);
                        // Ajustar se o dia não existe no mês (ex: 31 em fevereiro)
                        if (dataVencimento.Month != folha.Mes)
                        {
                            dataVencimento = new DateTime(folha.Ano, folha.Mes, DateTime.DaysInMonth(folha.Ano, folha.Mes));
                        }
                        lancamentosFolha.Add(CriarLancamentoFolha(lancamento, folha, dataVencimento, 1, 1));
                    }
                    break;

                case TipoRecorrencia.Parcelado:
                    // Calcular qual parcela seria neste mês
                    var mesesDesdeInicio = ((folha.Ano - lancamento.DataInicial.Year) * 12) + (folha.Mes - lancamento.DataInicial.Month);
                    if (mesesDesdeInicio >= 0 && mesesDesdeInicio < lancamento.QuantidadeParcelas)
                    {
                        var parcelaAtual = mesesDesdeInicio + 1;
                        var dataVencimentoParcela = new DateTime(folha.Ano, folha.Mes, lancamento.DataInicial.Day);
                        // Ajustar se o dia não existe no mês
                        if (dataVencimentoParcela.Month != folha.Mes)
                        {
                            dataVencimentoParcela = new DateTime(folha.Ano, folha.Mes, DateTime.DaysInMonth(folha.Ano, folha.Mes));
                        }
                        lancamentosFolha.Add(CriarLancamentoFolha(lancamento, folha, dataVencimentoParcela, parcelaAtual, lancamento.QuantidadeParcelas.Value));
                    }
                    break;
            }

            return lancamentosFolha;
        }

        private LancamentoFolha CriarLancamentoFolha(Lancamento lancamento, FolhaMensal folha, DateTime dataPrevista, int parcelaAtual, int totalParcelas)
        {
            var descricao = lancamento.Descricao;
            if (lancamento.TipoRecorrencia == TipoRecorrencia.Parcelado)
            {
                descricao += $" - {parcelaAtual}/{totalParcelas}";
            }

            return new LancamentoFolha
            {
                FolhaMensalId = folha.Id,
                LancamentoOrigemId = lancamento.Id,
                Descricao = descricao,
                ValorProvisionado = lancamento.ValorProvisionado,
                ValorReal = lancamento.ValorReal,
                DataPrevista = dataPrevista,
                DataRealizacao = lancamento.ValorReal.HasValue ? dataPrevista : null,
                Tipo = lancamento.Tipo,
                TipoRecorrencia = lancamento.TipoRecorrencia,
                ParcelaAtual = lancamento.TipoRecorrencia == TipoRecorrencia.Parcelado ? parcelaAtual : null,
                TotalParcelas = lancamento.TipoRecorrencia == TipoRecorrencia.Parcelado ? totalParcelas : null,
                CategoriaId = lancamento.CategoriaId,
                Realizado = lancamento.ValorReal.HasValue,
                DataCriacao = DateTime.UtcNow
            };
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

