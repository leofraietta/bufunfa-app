using Bufunfa.Api.Models;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Interface para processamento de lançamentos em folhas mensais
    /// Segue o princípio da Inversão de Dependência (SOLID)
    /// </summary>
    public interface ILancamentoProcessorService
    {
        /// <summary>
        /// Processa todos os lançamentos de uma conta para uma folha mensal específica
        /// </summary>
        Task<List<LancamentoFolha>> ProcessarLancamentosParaFolhaAsync(int usuarioId, int contaId, FolhaMensal folha);

        /// <summary>
        /// Processa um lançamento específico para uma folha mensal
        /// </summary>
        Task<List<LancamentoFolha>> ProcessarLancamentoParaFolhaAsync(Lancamento lancamento, FolhaMensal folha);

        /// <summary>
        /// Verifica se um lançamento deve ser processado em uma determinada folha
        /// </summary>
        bool DeveProcessarLancamentoNaFolha(Lancamento lancamento, FolhaMensal folha);

        /// <summary>
        /// Obtém as datas de vencimento de um lançamento dentro do período da folha
        /// </summary>
        IEnumerable<DateTime> ObterDatasVencimentoNaFolha(Lancamento lancamento, FolhaMensal folha);
    }
}
