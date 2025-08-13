using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Lançamento periódico - ocorre com periodicidade configurável
    /// Pode ser semanal, quinzenal, mensal, anual ou personalizada (N em N dias)
    /// </summary>
    public class LancamentoPeriodico : Lancamento
    {
        public LancamentoPeriodico()
        {
            TipoRecorrencia = TipoRecorrencia.Periodico;
        }

        // Nota: Este lançamento usa as propriedades da classe base:
        // - TipoPeriodicidade (obrigatório)
        // - IntervaloDias (para periodicidade personalizada)
        // - DiaVencimento (para periodicidade mensal)

        /// <summary>
        /// Para periodicidade anual - dia do ano (1-366)
        /// </summary>
        public int? DiaDoAno { get; set; }

        /// <summary>
        /// Para periodicidade semanal - dia da semana (0=Domingo, 1=Segunda, ..., 6=Sábado)
        /// </summary>
        public DayOfWeek? DiaDaSemana { get; set; }

        /// <summary>
        /// Obtém o intervalo em dias baseado no tipo de periodicidade
        /// </summary>
        [NotMapped]
        public int IntervaloDiasEfetivo
        {
            get
            {
                if (!TipoPeriodicidade.HasValue)
                    return 30;

                switch (TipoPeriodicidade.Value)
                {
                    case Models.TipoPeriodicidade.Semanal:
                        return 7;
                    case Models.TipoPeriodicidade.Quinzenal:
                        return 15;
                    case Models.TipoPeriodicidade.Mensal:
                        return 30; // Aproximado para cálculos gerais
                    case Models.TipoPeriodicidade.Anual:
                        return 365; // Aproximado para cálculos gerais
                    case Models.TipoPeriodicidade.Personalizado:
                        return IntervaloDias ?? 1;
                    default:
                        return 30;
                }
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

            // Deve ser antes da data final (se definida)
            if (DataFinal.HasValue && data.Date > DataFinal.Value.Date)
                return false;

            if (!TipoPeriodicidade.HasValue)
                return false;

            switch (TipoPeriodicidade.Value)
            {
                case Models.TipoPeriodicidade.Semanal:
                    return PodeSerProcessadoSemanal(data);
                case Models.TipoPeriodicidade.Quinzenal:
                    return PodeSerProcessadoQuinzenal(data);
                case Models.TipoPeriodicidade.Mensal:
                    return PodeSerProcessadoMensal(data);
                case Models.TipoPeriodicidade.Anual:
                    return PodeSerProcessadoAnual(data);
                case Models.TipoPeriodicidade.Personalizado:
                    return PodeSerProcessadoPersonalizado(data);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Obtém a próxima data de vencimento
        /// </summary>
        public override DateTime? ProximaDataVencimento(DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.Now;
            
            // Se ainda não chegou na data inicial
            if (referencia.Date < DataInicial.Date)
                return DataInicial;

            // Se já passou da data final
            if (DataFinal.HasValue && referencia.Date > DataFinal.Value.Date)
                return null;

            if (!TipoPeriodicidade.HasValue)
                return null;

            DateTime? proximaData = null;
            switch (TipoPeriodicidade.Value)
            {
                case Models.TipoPeriodicidade.Semanal:
                    proximaData = ObterProximaDataSemanal(referencia);
                    break;
                case Models.TipoPeriodicidade.Quinzenal:
                    proximaData = ObterProximaDataQuinzenal(referencia);
                    break;
                case Models.TipoPeriodicidade.Mensal:
                    proximaData = ObterProximaDataMensal(referencia);
                    break;
                case Models.TipoPeriodicidade.Anual:
                    proximaData = ObterProximaDataAnual(referencia);
                    break;
                case Models.TipoPeriodicidade.Personalizado:
                    proximaData = ObterProximaDataPersonalizada(referencia);
                    break;
                default:
                    proximaData = null;
                    break;
            }

            // Verifica se a próxima data não ultrapassa a data final
            if (proximaData.HasValue && DataFinal.HasValue && proximaData > DataFinal.Value.Date)
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

            if (!TipoPeriodicidade.HasValue)
                return Enumerable.Empty<DateTime>();

            switch (TipoPeriodicidade.Value)
            {
                case Models.TipoPeriodicidade.Semanal:
                    return ObterDatasVencimentoSemanal(dataAtual, dataLimite);
                case Models.TipoPeriodicidade.Quinzenal:
                    return ObterDatasVencimentoQuinzenal(dataAtual, dataLimite);
                case Models.TipoPeriodicidade.Mensal:
                    return ObterDatasVencimentoMensal(dataAtual, dataLimite);
                case Models.TipoPeriodicidade.Anual:
                    return ObterDatasVencimentoAnual(dataAtual, dataLimite);
                case Models.TipoPeriodicidade.Personalizado:
                    return ObterDatasVencimentoPersonalizada(dataAtual, dataLimite);
                default:
                    return Enumerable.Empty<DateTime>();
            }
        }

        /// <summary>
        /// Valida se o lançamento periódico está configurado corretamente
        /// </summary>
        public override bool EhValido()
        {
            if (TipoRecorrencia != TipoRecorrencia.Periodico)
                return false;

            if (!TipoPeriodicidade.HasValue)
                return false;

            switch (TipoPeriodicidade.Value)
            {
                case Models.TipoPeriodicidade.Semanal:
                    return DiaDaSemana.HasValue;
                case Models.TipoPeriodicidade.Quinzenal:
                    return true; // Usa data inicial como referência
                case Models.TipoPeriodicidade.Mensal:
                    return DiaVencimento.HasValue && DiaVencimento >= 1 && DiaVencimento <= 31;
                case Models.TipoPeriodicidade.Anual:
                    return DiaDoAno.HasValue && DiaDoAno >= 1 && DiaDoAno <= 366;
                case Models.TipoPeriodicidade.Personalizado:
                    return IntervaloDias.HasValue && IntervaloDias > 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Obtém descrição completa específica para lançamentos periódicos
        /// </summary>
        public override string ObterDescricaoCompleta()
        {
            var periodicidade = "Periódico";
            if (TipoPeriodicidade.HasValue)
            {
                switch (TipoPeriodicidade.Value)
                {
                    case Models.TipoPeriodicidade.Semanal:
                        periodicidade = $"Semanal ({DiaDaSemana})";
                        break;
                    case Models.TipoPeriodicidade.Quinzenal:
                        periodicidade = "Quinzenal";
                        break;
                    case Models.TipoPeriodicidade.Mensal:
                        periodicidade = $"Mensal (dia {DiaVencimento})";
                        break;
                    case Models.TipoPeriodicidade.Anual:
                        periodicidade = $"Anual (dia {DiaDoAno} do ano)";
                        break;
                    case Models.TipoPeriodicidade.Personalizado:
                        periodicidade = $"A cada {IntervaloDias} dias";
                        break;
                    default:
                        periodicidade = "Periódico";
                        break;
                }
            }

            var descricao = $"{Descricao} - {Tipo} ({periodicidade})";
            if (DataFinal.HasValue)
            {
                descricao += $" até {DataFinal.Value:dd/MM/yyyy}";
            }
            return descricao;
        }

        #region Métodos privados para cada tipo de periodicidade

        private bool PodeSerProcessadoSemanal(DateTime data)
        {
            return DiaDaSemana.HasValue && data.DayOfWeek == DiaDaSemana.Value;
        }

        private bool PodeSerProcessadoQuinzenal(DateTime data)
        {
            var diasDiferenca = (data.Date - DataInicial.Date).Days;
            return diasDiferenca >= 0 && diasDiferenca % 15 == 0;
        }

        private bool PodeSerProcessadoMensal(DateTime data)
        {
            if (!DiaVencimento.HasValue) return false;
            
            return data.Day == DiaVencimento.Value || 
                   (DiaVencimento.Value > DateTime.DaysInMonth(data.Year, data.Month) && 
                    data.Day == DateTime.DaysInMonth(data.Year, data.Month));
        }

        private bool PodeSerProcessadoAnual(DateTime data)
        {
            if (!DiaDoAno.HasValue) return false;
            
            return data.DayOfYear == DiaDoAno.Value ||
                   (DiaDoAno.Value > (DateTime.IsLeapYear(data.Year) ? 366 : 365) && 
                    data.DayOfYear == (DateTime.IsLeapYear(data.Year) ? 366 : 365));
        }

        private bool PodeSerProcessadoPersonalizado(DateTime data)
        {
            if (!IntervaloDias.HasValue) return false;
            
            var diasDiferenca = (data.Date - DataInicial.Date).Days;
            return diasDiferenca >= 0 && diasDiferenca % IntervaloDias.Value == 0;
        }

        private DateTime? ObterProximaDataSemanal(DateTime referencia)
        {
            if (!DiaDaSemana.HasValue) return null;
            
            var diasParaProximo = ((int)DiaDaSemana.Value - (int)referencia.DayOfWeek + 7) % 7;
            if (diasParaProximo == 0 && referencia.Date > DataInicial.Date) diasParaProximo = 7;
            
            return referencia.Date.AddDays(diasParaProximo);
        }

        private DateTime? ObterProximaDataQuinzenal(DateTime referencia)
        {
            var diasDiferenca = (referencia.Date - DataInicial.Date).Days;
            var proximoIntervalo = ((diasDiferenca / 15) + 1) * 15;
            return DataInicial.Date.AddDays(proximoIntervalo);
        }

        private DateTime? ObterProximaDataMensal(DateTime referencia)
        {
            if (!DiaVencimento.HasValue) return null;
            
            var ano = referencia.Year;
            var mes = referencia.Month;
            var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(ano, mes));
            var dataVencimento = new DateTime(ano, mes, diaDoMes);

            if (dataVencimento <= referencia.Date)
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

        private DateTime? ObterProximaDataAnual(DateTime referencia)
        {
            if (!DiaDoAno.HasValue) return null;
            
            var ano = referencia.Year;
            var dataVencimento = new DateTime(ano, 1, 1).AddDays(DiaDoAno.Value - 1);

            if (dataVencimento <= referencia.Date)
            {
                ano++;
                dataVencimento = new DateTime(ano, 1, 1).AddDays(DiaDoAno.Value - 1);
            }

            return dataVencimento;
        }

        private DateTime? ObterProximaDataPersonalizada(DateTime referencia)
        {
            if (!IntervaloDias.HasValue) return null;
            
            var diasDiferenca = (referencia.Date - DataInicial.Date).Days;
            var proximoIntervalo = ((diasDiferenca / IntervaloDias.Value) + 1) * IntervaloDias.Value;
            return DataInicial.Date.AddDays(proximoIntervalo);
        }

        private IEnumerable<DateTime> ObterDatasVencimentoSemanal(DateTime dataInicio, DateTime dataFim)
        {
            if (!DiaDaSemana.HasValue) yield break;
            
            var dataAtual = dataInicio;
            while (dataAtual <= dataFim)
            {
                if (dataAtual.DayOfWeek == DiaDaSemana.Value && dataAtual >= DataInicial.Date)
                {
                    yield return dataAtual;
                }
                dataAtual = dataAtual.AddDays(1);
            }
        }

        private IEnumerable<DateTime> ObterDatasVencimentoQuinzenal(DateTime dataInicio, DateTime dataFim)
        {
            var dataAtual = DataInicial.Date;
            while (dataAtual <= dataFim)
            {
                if (dataAtual >= dataInicio)
                {
                    yield return dataAtual;
                }
                dataAtual = dataAtual.AddDays(15);
            }
        }

        private IEnumerable<DateTime> ObterDatasVencimentoMensal(DateTime dataInicio, DateTime dataFim)
        {
            if (!DiaVencimento.HasValue) yield break;
            
            var dataAtual = new DateTime(dataInicio.Year, dataInicio.Month, 1);
            while (dataAtual <= dataFim)
            {
                var diaDoMes = Math.Min(DiaVencimento.Value, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));
                var dataVencimento = new DateTime(dataAtual.Year, dataAtual.Month, diaDoMes);

                if (dataVencimento >= dataInicio && dataVencimento >= DataInicial.Date)
                {
                    yield return dataVencimento;
                }

                dataAtual = dataAtual.AddMonths(1);
            }
        }

        private IEnumerable<DateTime> ObterDatasVencimentoAnual(DateTime dataInicio, DateTime dataFim)
        {
            if (!DiaDoAno.HasValue) yield break;
            
            var ano = dataInicio.Year;
            while (ano <= dataFim.Year)
            {
                var dataVencimento = new DateTime(ano, 1, 1).AddDays(DiaDoAno.Value - 1);
                if (dataVencimento >= dataInicio && dataVencimento <= dataFim && dataVencimento >= DataInicial.Date)
                {
                    yield return dataVencimento;
                }
                ano++;
            }
        }

        private IEnumerable<DateTime> ObterDatasVencimentoPersonalizada(DateTime dataInicio, DateTime dataFim)
        {
            if (!IntervaloDias.HasValue) yield break;
            
            var dataAtual = DataInicial.Date;
            while (dataAtual <= dataFim)
            {
                if (dataAtual >= dataInicio)
                {
                    yield return dataAtual;
                }
                dataAtual = dataAtual.AddDays(IntervaloDias.Value);
            }
        }

        #endregion
    }
}
