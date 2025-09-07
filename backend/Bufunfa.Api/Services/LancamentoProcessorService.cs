using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço responsável pelo processamento de lançamentos em folhas mensais
    /// Implementa as regras de negócio específicas para cada tipo de lançamento
    /// Segue os princípios SOLID e Strategy Pattern
    /// </summary>
    public class LancamentoProcessorService : ILancamentoProcessorService
    {
        private readonly ApplicationDbContext _context;

        public LancamentoProcessorService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Processa todos os lançamentos de uma conta para uma folha mensal específica
        /// </summary>
        public async Task<List<LancamentoFolha>> ProcessarLancamentosParaFolhaAsync(int usuarioId, int contaId, FolhaMensal folha)
        {
            var lancamentos = await _context.Lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.UsuarioId == usuarioId && l.ContaId == contaId && l.Ativo)
                .ToListAsync();

            var lancamentosFolha = new List<LancamentoFolha>();

            foreach (var lancamento in lancamentos)
            {
                var lancamentosGerados = await ProcessarLancamentoParaFolhaAsync(lancamento, folha);
                lancamentosFolha.AddRange(lancamentosGerados);
            }

            return lancamentosFolha;
        }

        /// <summary>
        /// Processa um lançamento específico para uma folha mensal
        /// Usa polimorfismo para aplicar regras específicas de cada tipo
        /// </summary>
        public async Task<List<LancamentoFolha>> ProcessarLancamentoParaFolhaAsync(Lancamento lancamento, FolhaMensal folha)
        {
            var lancamentosFolha = new List<LancamentoFolha>();

            if (!DeveProcessarLancamentoNaFolha(lancamento, folha))
                return lancamentosFolha;

            var datasVencimento = ObterDatasVencimentoNaFolha(lancamento, folha);

            foreach (var dataVencimento in datasVencimento)
            {
                var lancamentoFolha = await CriarLancamentoFolhaAsync(lancamento, folha, dataVencimento);
                if (lancamentoFolha != null)
                {
                    lancamentosFolha.Add(lancamentoFolha);
                }
            }

            return lancamentosFolha;
        }

        /// <summary>
        /// Verifica se um lançamento deve ser processado em uma determinada folha
        /// </summary>
        public bool DeveProcessarLancamentoNaFolha(Lancamento lancamento, FolhaMensal folha)
        {
            var dataInicioFolha = DateTime.SpecifyKind(new DateTime(folha.Ano, folha.Mes, 1), DateTimeKind.Utc);
            var dataFimFolha = dataInicioFolha.AddMonths(1).AddDays(-1);

            // Verifica se há interseção entre o período do lançamento e o período da folha
            var lancamentoInicia = lancamento.DataInicial.Date;
            var lancamentoTermina = lancamento.DataFinal?.Date ?? DateTime.SpecifyKind(DateTime.MaxValue.Date, DateTimeKind.Utc);

            return lancamentoInicia <= dataFimFolha && lancamentoTermina >= dataInicioFolha;
        }

        /// <summary>
        /// Obtém as datas de vencimento de um lançamento dentro do período da folha
        /// Usa polimorfismo para aplicar lógica específica de cada tipo
        /// </summary>
        public IEnumerable<DateTime> ObterDatasVencimentoNaFolha(Lancamento lancamento, FolhaMensal folha)
        {
            var dataInicioFolha = DateTime.SpecifyKind(new DateTime(folha.Ano, folha.Mes, 1), DateTimeKind.Utc);
            var dataFimFolha = dataInicioFolha.AddMonths(1).AddDays(-1);

            return lancamento.ObterDatasVencimento(dataInicioFolha, dataFimFolha);
        }

        /// <summary>
        /// Cria um lançamento de folha baseado no lançamento origem e data específica
        /// </summary>
        private async Task<LancamentoFolha> CriarLancamentoFolhaAsync(Lancamento lancamento, FolhaMensal folha, DateTime dataVencimento)
        {
            // Verifica se já existe lançamento para esta data (evita duplicatas)
            var jaExiste = await _context.LancamentosFolha
                .AnyAsync(lf => lf.FolhaMensalId == folha.Id && 
                               lf.LancamentoOrigemId == lancamento.Id && 
                               lf.DataPrevista.Date == dataVencimento.ToUniversalTime().Date);

            if (jaExiste)
                return null;

            var descricao = ObterDescricaoLancamentoFolha(lancamento, dataVencimento, folha);
            var (parcelaAtual, totalParcelas) = ObterInformacoesParcela(lancamento, dataVencimento);

            var lancamentoFolha = new LancamentoFolha
            {
                FolhaMensalId = folha.Id,
                LancamentoOrigemId = lancamento.Id,
                Descricao = descricao,
                ValorProvisionado = ObterValorProvisionado(lancamento, parcelaAtual),
                ValorReal = lancamento.ValorReal,
                DataPrevista = dataVencimento,
                DataRealizacao = lancamento.ValorReal.HasValue ? dataVencimento : null,
                Tipo = lancamento.Tipo,
                TipoRecorrencia = lancamento.TipoRecorrencia,
                ParcelaAtual = parcelaAtual,
                TotalParcelas = totalParcelas,
                CategoriaId = lancamento.CategoriaId,
                Realizado = lancamento.ValorReal.HasValue,
                DataCriacao = DateTime.UtcNow
            };

            return lancamentoFolha;
        }

        /// <summary>
        /// Obtém a descrição específica para o lançamento na folha
        /// </summary>
        private string ObterDescricaoLancamentoFolha(Lancamento lancamento, DateTime dataVencimento, FolhaMensal folha)
        {
            var descricao = lancamento.Descricao;

            switch (lancamento.TipoRecorrencia)
            {
                case TipoRecorrencia.Parcelado:
                    var (parcelaAtual, totalParcelas) = ObterInformacoesParcela(lancamento, dataVencimento);
                    if (parcelaAtual.HasValue && totalParcelas.HasValue)
                    {
                        descricao += $" - {parcelaAtual}/{totalParcelas}";
                    }
                    break;

                // Periodico foi removido - agora é parte de Recorrente

                case TipoRecorrencia.Recorrente:
                    descricao += $" - Recorrente (dia {lancamento.DiaVencimento})";
                    break;

                case TipoRecorrencia.Esporadico:
                    descricao += $" - Esporádico";
                    break;
            }

            return descricao;
        }

        /// <summary>
        /// Obtém informações de parcela para lançamentos parcelados
        /// </summary>
        private (int? ParcelaAtual, int? TotalParcelas) ObterInformacoesParcela(Lancamento lancamento, DateTime dataVencimento)
        {
            if (lancamento.TipoRecorrencia != TipoRecorrencia.Parcelado || !lancamento.QuantidadeParcelas.HasValue)
                return (null, null);

            var mesesDiferenca = ((dataVencimento.Year - lancamento.DataInicial.Year) * 12) + 
                                (dataVencimento.Month - lancamento.DataInicial.Month);
            var parcelaAtual = mesesDiferenca + 1;

            return parcelaAtual <= lancamento.QuantidadeParcelas ? 
                (parcelaAtual, lancamento.QuantidadeParcelas) : 
                (null, null);
        }

        /// <summary>
        /// Obtém o valor provisionado considerando parcelas
        /// </summary>
        private decimal ObterValorProvisionado(Lancamento lancamento, int? parcelaAtual)
        {
            if (lancamento.TipoRecorrencia == TipoRecorrencia.Parcelado && 
                lancamento.QuantidadeParcelas.HasValue && 
                lancamento is LancamentoParcelado lancamentoParcelado)
            {
                return lancamentoParcelado.ValorEfetivoParcela;
            }

            return lancamento.ValorProvisionado;
        }

        /// <summary>
        /// Obtém descrição da periodicidade para lançamentos recorrentes
        /// </summary>
        private string ObterDescricaoPeriodicidade(LancamentoRecorrente lancamentoRecorrente)
        {
            if (!lancamentoRecorrente.TipoPeriodicidade.HasValue)
                return "Recorrente";

            return lancamentoRecorrente.TipoPeriodicidade.Value switch
            {
                TipoPeriodicidade.Semanal => $"Semanal ({lancamentoRecorrente.DiaDaSemana})",
                TipoPeriodicidade.Quinzenal => "Quinzenal",
                TipoPeriodicidade.Mensal => $"Mensal (dia {lancamentoRecorrente.DiaVencimento})",
                TipoPeriodicidade.Anual => $"Anual (dia {lancamentoRecorrente.DiaDoAno} do ano)",
                TipoPeriodicidade.Personalizado => $"A cada {lancamentoRecorrente.IntervaloDias} dias",
                _ => "Periódico"
            };
        }
    }
}
