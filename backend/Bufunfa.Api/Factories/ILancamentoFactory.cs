using Bufunfa.Api.Models;

namespace Bufunfa.Api.Factories
{
    /// <summary>
    /// Interface para Factory de Lançamentos
    /// Segue o padrão Factory Method para criação de instâncias especializadas
    /// </summary>
    public interface ILancamentoFactory
    {
        /// <summary>
        /// Cria uma instância de lançamento baseada no tipo de recorrência
        /// </summary>
        Lancamento CriarLancamento(TipoRecorrencia tipoRecorrencia);

        /// <summary>
        /// Cria um lançamento esporádico
        /// </summary>
        LancamentoEsporadico CriarLancamentoEsporadico();

        /// <summary>
        /// Cria um lançamento recorrente
        /// </summary>
        LancamentoRecorrente CriarLancamentoRecorrente();

        /// <summary>
        /// Cria um lançamento parcelado
        /// </summary>
        LancamentoParcelado CriarLancamentoParcelado();

        // CriarLancamentoPeriodico removido - funcionalidade integrada em LancamentoRecorrente

        /// <summary>
        /// Valida se os parâmetros são válidos para o tipo de lançamento
        /// </summary>
        bool ValidarParametros(TipoRecorrencia tipo, object parametros);
    }
}
