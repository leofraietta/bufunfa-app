using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Lançamento recorrente - ocorre todo dia X de cada mês
    /// Pode ter data final opcional para encerrar a recorrência
    /// </summary>
    public class LancamentoRecorrente : Lancamento
    {
        public LancamentoRecorrente()
        {
            TipoRecorrencia = TipoRecorrencia.Recorrente;
        }

        // Nota: Este lançamento usa as propriedades da classe base:
        // - DiaVencimento (obrigatório para recorrentes)

        /// <summary>
        /// Verifica se o lançamento pode ser processado em uma determinada data
        /// </summary>
        public override bool PodeSerProcessadoEm(DateTime data)
        {
            // Deve ser após ou na data inicial
            if (data.Date < DataInicial.Date)
                return false;

            // Deve ser antes da data final (se definida)
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
            var dataAtual = new DateTime(Math.Max(dataInicio.Ticks, DataInicial.Ticks));
            var dataLimite = DataFinal.HasValue ? 
                new DateTime(Math.Min(dataFim.Ticks, DataFinal.Value.Ticks)) : 
                dataFim;

            while (dataAtual <= dataLimite)
            {
                var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));
                var dataVencimento = new DateTime(dataAtual.Year, dataAtual.Month, diaDoMes);

                if (dataVencimento >= dataInicio && dataVencimento <= dataLimite)
                {
                    yield return dataVencimento;
                }

                // Próximo mês
                dataAtual = dataAtual.AddMonths(1);
                dataAtual = new DateTime(dataAtual.Year, dataAtual.Month, 1);
            }
        }

        /// <summary>
        /// Valida se o lançamento recorrente está configurado corretamente
        /// </summary>
        public override bool EhValido()
        {
            return TipoRecorrencia == TipoRecorrencia.Recorrente &&
                   DiaVencimento >= 1 && DiaVencimento <= 31 &&
                   !QuantidadeParcelas.HasValue && // Recorrentes não têm parcelas
                   !TipoPeriodicidade.HasValue && // Recorrentes não têm periodicidade
                   !IntervaloDias.HasValue &&
                   (DataFinal == null || DataFinal >= DataInicial); // Data final deve ser posterior à inicial
        }

        /// <summary>
        /// Obtém descrição completa específica para lançamentos recorrentes
        /// </summary>
        public override string ObterDescricaoCompleta()
        {
            var descricao = $"{Descricao} - {Tipo} (todo dia {DiaVencimento})";
            if (DataFinal.HasValue)
            {
                descricao += $" até {DataFinal.Value:dd/MM/yyyy}";
            }
            return descricao;
        }

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
        /// Verifica se está ativo na data especificada
        /// </summary>
        public bool EstaAtivoEm(DateTime data)
        {
            return data.Date >= DataInicial.Date && 
                   (!DataFinal.HasValue || data.Date <= DataFinal.Value.Date);
        }
    }
}
