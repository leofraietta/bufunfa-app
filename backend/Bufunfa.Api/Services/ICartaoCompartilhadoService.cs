using Bufunfa.Api.Models;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Interface para serviços de cartões de crédito compartilhados
    /// Gerencia cartões vinculados a contas conjuntas com múltiplos usuários
    /// </summary>
    public interface ICartaoCompartilhadoService
    {
        /// <summary>
        /// Consolida a fatura do cartão compartilhado na conta responsável
        /// </summary>
        Task<ConsolidacaoFaturaResult> ConsolidarFaturaAsync(int cartaoId, int ano, int mes);

        /// <summary>
        /// Verifica se um usuário pode usar o cartão compartilhado
        /// </summary>
        bool UsuarioPodeUsarCartao(int usuarioId, int cartaoId);

        /// <summary>
        /// Adiciona um usuário autorizado ao cartão compartilhado
        /// </summary>
        Task AdicionarUsuarioAutorizadoAsync(int cartaoId, int usuarioId);

        /// <summary>
        /// Remove um usuário autorizado do cartão compartilhado
        /// </summary>
        Task RemoverUsuarioAutorizadoAsync(int cartaoId, int usuarioId);

        /// <summary>
        /// Obtém todos os usuários autorizados de um cartão
        /// </summary>
        Task<List<Usuario>> ObterUsuariosAutorizadosAsync(int cartaoId);

        /// <summary>
        /// Calcula o valor da fatura consolidada para um período
        /// </summary>
        Task<decimal> CalcularValorFaturaConsolidadaAsync(int cartaoId, int ano, int mes);

        /// <summary>
        /// Obtém o detalhamento da fatura por usuário
        /// </summary>
        Task<Dictionary<int, decimal>> ObterDetalhamentoFaturaPorUsuarioAsync(int cartaoId, int ano, int mes);
    }

    /// <summary>
    /// Resultado da consolidação de fatura
    /// </summary>
    public class ConsolidacaoFaturaResult
    {
        public int CartaoId { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorTotal { get; set; }
        public Dictionary<int, decimal> DetalhamentoPorUsuario { get; set; } = new();
        public int? LancamentoFolhaId { get; set; }
        public bool Consolidada { get; set; }
    }
}
