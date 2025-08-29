using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Lançamento parcelado - ocorre todo dia X por N meses consecutivos
    /// A data final é calculada automaticamente baseada na quantidade de parcelas
    /// </summary>
    public class LancamentoParcelado : Lancamento
    {
        public LancamentoParcelado()
        {
            TipoRecorrencia = TipoRecorrencia.Parcelado;
        }

        // Nota: Este lançamento usa as propriedades da classe base:
        // - QuantidadeParcelas (obrigatório)
        // - DiaVencimento (obrigatório)

        /// <summary>
        /// Valor de cada parcela (pode ser diferente do valor provisionado para casos de juros/multa)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorParcela { get; set; }

        /// <summary>
        /// Indica se permite alteração do valor real de parcelas individuais
        /// </summary>
        public bool PermiteAlteracaoValorParcela { get; set; } = true;

        /// <summary>
        /// Valor efetivo de cada parcela (considerando valor real ou provisionado)
        /// </summary>
        [NotMapped]
        public decimal ValorEfetivoParcela => ValorParcela ?? (ValorProvisionado / QuantidadeParcelas.Value);

        /// <summary>
        /// Calcula automaticamente a data final baseada na quantidade de parcelas
        /// </summary>
        public override void CalcularDataFinal()
        {
            if (QuantidadeParcelas > 0)
            {
                var dataFinalCalculada = DataInicial.AddMonths(QuantidadeParcelas.Value - 1);
                var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(dataFinalCalculada.Year, dataFinalCalculada.Month));
                DataFinal = new DateTime(dataFinalCalculada.Year, dataFinalCalculada.Month, diaDoMes);
            }
        }

        /// <summary>
        /// Verifica se o lançamento pode ser processado em uma determinada data
        /// </summary>
        public override bool PodeSerProcessadoEm(DateTime data)
        {
            // Deve ser após ou na data inicial
            if (data.Date < DataInicial.Date)
                return false;

            // Deve ser antes da data final calculada
            if (DataFinal.HasValue && data.Date > DataFinal.Value.Date)
                return false;

            // Deve ser no dia correto do mês
            return data.Day == DiaVencimento || 
                   (DiaVencimento > DateTime.DaysInMonth(data.Year, data.Month) && 
                    data.Day == DateTime.DaysInMonth(data.Year, data.Month));
        }

        /// <summary>
        /// Obtém a próxima data de vencimento
        /// </summary>
        public override DateTime? ProximaDataVencimento(DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.UtcNow;
            
            // Se ainda não chegou na data inicial
            if (referencia.Date < DataInicial.Date)
                return DataInicial;

            // Se já passou da data final
            if (DataFinal.HasValue && referencia.Date > DataFinal.Value.Date)
                return null;

            var proximaData = ObterProximaDataVencimento(referencia);
            
            // Verifica se a próxima data não ultrapassa a data final
            if (DataFinal.HasValue && proximaData > DataFinal.Value.Date)
                return null;

            return proximaData;
        }

        /// <summary>
        /// Obtém todas as datas de vencimento dentro de um período
        /// </summary>
        public override IEnumerable<DateTime> ObterDatasVencimento(DateTime dataInicio, DateTime dataFim)
        {
            // Garante que a data final está calculada
            if (!DataFinal.HasValue)
                CalcularDataFinal();

            var dataAtual = new DateTime(Math.Max(dataInicio.Ticks, DataInicial.Ticks));
            var dataLimite = DataFinal.HasValue ? 
                new DateTime(Math.Min(dataFim.Ticks, DataFinal.Value.Ticks)) : 
                dataFim;

            var parcelaAtual = 1;
            var mesAtual = DataInicial;

            while (parcelaAtual <= QuantidadeParcelas && mesAtual <= dataLimite)
            {
                var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(mesAtual.Year, mesAtual.Month));
                var dataVencimento = new DateTime(mesAtual.Year, mesAtual.Month, diaDoMes);

                if (dataVencimento >= dataInicio && dataVencimento <= dataLimite)
                {
                    yield return dataVencimento;
                }

                parcelaAtual++;
                mesAtual = mesAtual.AddMonths(1);
            }
        }

        /// <summary>
        /// Valida se o lançamento parcelado está configurado corretamente
        /// </summary>
        public override bool EhValido()
        {
            return TipoRecorrencia == TipoRecorrencia.Parcelado &&
                   QuantidadeParcelas > 0 &&
                   DiaVencimento >= 1 && DiaVencimento <= 31 &&
                   !TipoPeriodicidade.HasValue && // Parcelados não têm periodicidade
                   !IntervaloDias.HasValue &&
                   (ValorParcela == null || ValorParcela > 0);
        }

        /// <summary>
        /// Obtém descrição completa específica para lançamentos parcelados
        /// </summary>
        public override string ObterDescricaoCompleta()
        {
            return $"{Descricao} - {Tipo} ({QuantidadeParcelas}x de {ValorEfetivoParcela:C} - dia {DiaVencimento})";
        }

        /// <summary>
        /// Obtém o número da parcela para uma determinada data
        /// </summary>
        public int? ObterNumeroParcela(DateTime data)
        {
            if (!PodeSerProcessadoEm(data))
                return null;

            var mesesDiferenca = ((data.Year - DataInicial.Year) * 12) + data.Month - DataInicial.Month;
            var numeroParcela = mesesDiferenca + 1;

            return numeroParcela <= QuantidadeParcelas ? numeroParcela : null;
        }

        /// <summary>
        /// Verifica se todas as parcelas já foram processadas
        /// </summary>
        [NotMapped]
        public bool TodasParcelasProcessadas => 
            LancamentosFolha.Count >= QuantidadeParcelas;

        /// <summary>
        /// Obtém a próxima data de vencimento a partir de uma data de referência
        /// </summary>
        private DateTime ObterProximaDataVencimento(DateTime dataReferencia)
        {
            var ano = dataReferencia.Year;
            var mes = dataReferencia.Month;
            var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(ano, mes));
            var dataVencimento = new DateTime(ano, mes, diaDoMes);

            // Se já passou do dia no mês atual, vai para o próximo mês
            if (dataVencimento <= dataReferencia.Date)
            {
                if (mes == 12)
                {
                    ano++;
                    mes = 1;
                }
                else
                {
                    mes++;
                }
                
                diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(ano, mes));
                dataVencimento = new DateTime(ano, mes, diaDoMes);
            }

            return dataVencimento;
        }

        /// <summary>
        /// Calcula o valor total do parcelamento
        /// </summary>
        [NotMapped]
        public decimal ValorTotalParcelamento => ValorEfetivoParcela * QuantidadeParcelas.Value;
    }
}
