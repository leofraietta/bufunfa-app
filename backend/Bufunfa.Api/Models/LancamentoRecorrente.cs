using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Lançamento recorrente - ocorre com periodicidade configurável
    /// Pode ser mensal (todo dia X), semanal, quinzenal, etc.
    /// Pode ter data final opcional para encerrar a recorrência
    /// </summary>
    public class LancamentoRecorrente : Lancamento
    {
        public LancamentoRecorrente()
        {
            TipoRecorrencia = TipoRecorrencia.Recorrente;
        }

        // Propriedades específicas para periodicidade expandida
        public DayOfWeek? DiaDaSemana { get; set; }
        public int? DiaDoAno { get; set; }

        // Nota: Este lançamento usa as propriedades da classe base:
        // - DiaVencimento (para periodicidade mensal)
        // - TipoPeriodicidade (tipo de recorrência)
        // - IntervaloDias (para periodicidade personalizada)

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

            // Verifica baseado no tipo de periodicidade
            if (!TipoPeriodicidade.HasValue)
            {
                // Recorrência mensal tradicional
                return data.Day == DiaVencimento || 
                       (DiaVencimento > DateTime.DaysInMonth(data.Year, data.Month) && 
                        data.Day == DateTime.DaysInMonth(data.Year, data.Month));
            }

            var tipoPeriodicidade = TipoPeriodicidade.Value;
            
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Semanal)
                return DiaDaSemana.HasValue && data.DayOfWeek == DiaDaSemana.Value;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Quinzenal)
                return VerificarPeriodicidadeQuinzenal(data);
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Mensal)
                return data.Day == DiaVencimento || 
                       (DiaVencimento > DateTime.DaysInMonth(data.Year, data.Month) && 
                        data.Day == DateTime.DaysInMonth(data.Year, data.Month));
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Bimestral)
                return VerificarPeriodicidadeBimestral(data);
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Trimestral)
                return VerificarPeriodicidadeTrimestral(data);
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Semestral)
                return VerificarPeriodicidadeSemestral(data);
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Anual)
                return DiaDoAno.HasValue && data.DayOfYear == DiaDoAno.Value;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.TodoDiaUtil)
                return VerificarDiaUtil(data);
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Personalizado)
                return VerificarPeriodicidadePersonalizada(data);
                
            return false;
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
            if (TipoRecorrencia != TipoRecorrencia.Recorrente)
                return false;

            if (QuantidadeParcelas.HasValue) // Recorrentes não têm parcelas
                return false;

            if (DataFinal.HasValue && DataFinal < DataInicial) // Data final deve ser posterior à inicial
                return false;

            // Validação baseada no tipo de periodicidade
            if (!TipoPeriodicidade.HasValue)
            {
                // Recorrência mensal tradicional
                return DiaVencimento >= 1 && DiaVencimento <= 31;
            }

            var tipoPeriodicidade = TipoPeriodicidade.Value;
            
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Semanal)
                return DiaDaSemana.HasValue;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Quinzenal)
                return true; // Quinzenal não precisa de parâmetros extras
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Mensal)
                return DiaVencimento >= 1 && DiaVencimento <= 31;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Bimestral)
                return DiaVencimento >= 1 && DiaVencimento <= 31;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Trimestral)
                return DiaVencimento >= 1 && DiaVencimento <= 31;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Semestral)
                return DiaVencimento >= 1 && DiaVencimento <= 31;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Anual)
                return DiaDoAno >= 1 && DiaDoAno <= 366;
            if (tipoPeriodicidade == Models.TipoPeriodicidade.TodoDiaUtil)
                return true; // Não precisa de parâmetros extras
            if (tipoPeriodicidade == Models.TipoPeriodicidade.Personalizado)
                return IntervaloDias >= 1 && IntervaloDias <= 365;
                
            return false;
        }

        /// <summary>
        /// Verifica periodicidade quinzenal (a cada 15 dias)
        /// </summary>
        private bool VerificarPeriodicidadeQuinzenal(DateTime data)
        {
            var diasDiferenca = (data.Date - DataInicial.Date).Days;
            return diasDiferenca >= 0 && diasDiferenca % 15 == 0;
        }

        /// <summary>
        /// Verifica periodicidade personalizada (a cada N dias)
        /// </summary>
        private bool VerificarPeriodicidadePersonalizada(DateTime data)
        {
            if (!IntervaloDias.HasValue) return false;
            
            var diasDiferenca = (data.Date - DataInicial.Date).Days;
            return diasDiferenca >= 0 && diasDiferenca % IntervaloDias.Value == 0;
        }

        /// <summary>
        /// Verifica periodicidade bimestral (a cada 2 meses)
        /// </summary>
        private bool VerificarPeriodicidadeBimestral(DateTime data)
        {
            if (!DiaVencimento.HasValue) return false;
            
            var mesesDiferenca = ((data.Year - DataInicial.Year) * 12) + data.Month - DataInicial.Month;
            var diaCorreto = data.Day == DiaVencimento.Value || 
                           (DiaVencimento.Value > DateTime.DaysInMonth(data.Year, data.Month) && 
                            data.Day == DateTime.DaysInMonth(data.Year, data.Month));
            
            return mesesDiferenca >= 0 && mesesDiferenca % 2 == 0 && diaCorreto;
        }

        /// <summary>
        /// Verifica periodicidade trimestral (a cada 3 meses)
        /// </summary>
        private bool VerificarPeriodicidadeTrimestral(DateTime data)
        {
            if (!DiaVencimento.HasValue) return false;
            
            var mesesDiferenca = ((data.Year - DataInicial.Year) * 12) + data.Month - DataInicial.Month;
            var diaCorreto = data.Day == DiaVencimento.Value || 
                           (DiaVencimento.Value > DateTime.DaysInMonth(data.Year, data.Month) && 
                            data.Day == DateTime.DaysInMonth(data.Year, data.Month));
            
            return mesesDiferenca >= 0 && mesesDiferenca % 3 == 0 && diaCorreto;
        }

        /// <summary>
        /// Verifica periodicidade semestral (a cada 6 meses)
        /// </summary>
        private bool VerificarPeriodicidadeSemestral(DateTime data)
        {
            if (!DiaVencimento.HasValue) return false;
            
            var mesesDiferenca = ((data.Year - DataInicial.Year) * 12) + data.Month - DataInicial.Month;
            var diaCorreto = data.Day == DiaVencimento.Value || 
                           (DiaVencimento.Value > DateTime.DaysInMonth(data.Year, data.Month) && 
                            data.Day == DateTime.DaysInMonth(data.Year, data.Month));
            
            return mesesDiferenca >= 0 && mesesDiferenca % 6 == 0 && diaCorreto;
        }

        /// <summary>
        /// Verifica se é dia útil (segunda a sexta-feira)
        /// </summary>
        private bool VerificarDiaUtil(DateTime data)
        {
            return data.DayOfWeek >= DayOfWeek.Monday && data.DayOfWeek <= DayOfWeek.Friday;
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
