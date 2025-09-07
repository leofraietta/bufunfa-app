using System;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Serviço para cálculos de dias úteis e ajustes de datas
    /// Implementa as regras de ajuste para finais de semana definidas nos requisitos
    /// </summary>
    public interface IBusinessDayService
    {
        DateTime AdjustToNextBusinessDay(DateTime date);
        bool IsBusinessDay(DateTime date);
        DateTime CalculateNextOccurrence(DateTime baseDate, TipoPeriodicidade pattern, int? customInterval = null);
        IEnumerable<DateTime> GenerateBusinessDaysInMonth(int year, int month);
    }

    public class BusinessDayService : IBusinessDayService
    {
        /// <summary>
        /// Ajusta uma data para o próximo dia útil se cair em final de semana
        /// </summary>
        public DateTime AdjustToNextBusinessDay(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        /// <summary>
        /// Verifica se uma data é dia útil (não é sábado nem domingo)
        /// </summary>
        public bool IsBusinessDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Calcula a próxima ocorrência baseada no padrão de periodicidade
        /// </summary>
        public DateTime CalculateNextOccurrence(DateTime baseDate, TipoPeriodicidade pattern, int? customInterval = null)
        {
            DateTime nextDate = pattern switch
            {
                TipoPeriodicidade.Semanal => baseDate.AddDays(7),
                TipoPeriodicidade.Quinzenal => baseDate.AddDays(14),
                TipoPeriodicidade.Mensal => baseDate.AddMonths(1),
                TipoPeriodicidade.Bimestral => baseDate.AddMonths(2),
                TipoPeriodicidade.Trimestral => baseDate.AddMonths(3),
                TipoPeriodicidade.Semestral => baseDate.AddMonths(6),
                TipoPeriodicidade.Anual => baseDate.AddYears(1),
                TipoPeriodicidade.Personalizado => baseDate.AddDays(customInterval ?? 1),
                TipoPeriodicidade.TodoDiaUtil => CalculateNextBusinessDay(baseDate),
                _ => throw new ArgumentException($"Padrão de periodicidade desconhecido: {pattern}")
            };

            // Ajustar para dia útil se necessário (exceto para TodoDiaUtil que já é calculado como dia útil)
            if (pattern != TipoPeriodicidade.TodoDiaUtil)
            {
                nextDate = AdjustToNextBusinessDay(nextDate);
            }

            return nextDate;
        }

        /// <summary>
        /// Gera todos os dias úteis de um mês
        /// </summary>
        public IEnumerable<DateTime> GenerateBusinessDaysInMonth(int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                if (IsBusinessDay(date))
                {
                    yield return date;
                }
            }
        }

        /// <summary>
        /// Calcula o próximo dia útil após a data base
        /// </summary>
        private DateTime CalculateNextBusinessDay(DateTime baseDate)
        {
            var nextDate = baseDate.AddDays(1);
            return AdjustToNextBusinessDay(nextDate);
        }

        /// <summary>
        /// Valida se o intervalo customizado está dentro dos limites permitidos (1-6 dias)
        /// </summary>
        public bool IsValidCustomInterval(int? interval)
        {
            return interval.HasValue && interval.Value >= 1 && interval.Value <= 6;
        }
    }
}
