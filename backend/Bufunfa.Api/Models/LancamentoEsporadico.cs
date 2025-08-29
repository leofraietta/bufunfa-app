using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Lançamento esporádico - feito diretamente na folha do mês vigente
    /// Não possui recorrência e é processado apenas uma vez
    /// </summary>
    public class LancamentoEsporadico : Lancamento
    {
        public LancamentoEsporadico()
        {
            TipoRecorrencia = TipoRecorrencia.Esporadico;
        }

        /// <summary>
        /// Verifica se o lançamento pode ser processado em uma determinada data
        /// Para esporádicos, só pode ser processado na data inicial
        /// </summary>
        public override bool PodeSerProcessadoEm(DateTime data)
        {
            return data.Date == DataInicial.Date;
        }

        /// <summary>
        /// Obtém a próxima data de vencimento
        /// Para esporádicos, retorna null se já passou da data inicial
        /// </summary>
        public override DateTime? ProximaDataVencimento(DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.UtcNow;
            return referencia.Date <= DataInicial.Date ? DataInicial : null;
        }

        /// <summary>
        /// Obtém todas as datas de vencimento dentro de um período
        /// Para esporádicos, retorna apenas a data inicial se estiver no período
        /// </summary>
        public override IEnumerable<DateTime> ObterDatasVencimento(DateTime dataInicio, DateTime dataFim)
        {
            if (DataInicial.Date >= dataInicio.Date && DataInicial.Date <= dataFim.Date)
            {
                yield return DataInicial;
            }
        }

        /// <summary>
        /// Valida se o lançamento esporádico está configurado corretamente
        /// </summary>
        public override bool EhValido()
        {
            // Lançamentos esporádicos não devem ter propriedades de recorrência
            return TipoRecorrencia == TipoRecorrencia.Esporadico &&
                   !QuantidadeParcelas.HasValue &&
                   !DiaVencimento.HasValue &&
                   !TipoPeriodicidade.HasValue &&
                   !IntervaloDias.HasValue &&
                   !DataFinal.HasValue; // Esporádicos não têm data final
        }

        /// <summary>
        /// Obtém descrição completa específica para lançamentos esporádicos
        /// </summary>
        public override string ObterDescricaoCompleta()
        {
            return $"{Descricao} - {Tipo} (Esporádico em {DataInicial:dd/MM/yyyy})";
        }

        /// <summary>
        /// Verifica se o lançamento já foi processado
        /// </summary>
        [NotMapped]
        public bool JaFoiProcessado => UltimaDataProcessamento.HasValue;

        /// <summary>
        /// Marca o lançamento como processado
        /// </summary>
        public void MarcarComoProcessado()
        {
            UltimaDataProcessamento = DateTime.UtcNow;
        }
    }
}
