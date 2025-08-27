using Bufunfa.Api.Models;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Interface para serviços de gestão de contas conjuntas
    /// Responsável por operações específicas de contas compartilhadas
    /// </summary>
    public interface IContaConjuntaService
    {
        /// <summary>
        /// Processa a apuração mensal da conta conjunta e distribui rateios
        /// </summary>
        Task<ApuracaoResult> ProcessarApuracaoMensalAsync(int contaConjuntaId, int ano, int mes);

        /// <summary>
        /// Distribui o rateio da conta conjunta nas folhas individuais dos usuários
        /// </summary>
        Task DistribuirRateioNasFolhasAsync(ContaConjunta contaConjunta, ApuracaoResult apuracao);

        /// <summary>
        /// Verifica se um usuário tem permissão para uma operação específica na conta
        /// </summary>
        bool UsuarioTemPermissao(int usuarioId, int contaId, TipoPermissao permissao);

        /// <summary>
        /// Adiciona um usuário à conta conjunta com permissões específicas
        /// </summary>
        Task<ContaUsuario> AdicionarUsuarioAsync(int contaConjuntaId, int usuarioId, decimal percentualParticipacao, 
            bool podeLer = true, bool podeEscrever = false, bool podeAdministrar = false);

        /// <summary>
        /// Remove um usuário da conta conjunta
        /// </summary>
        Task RemoverUsuarioAsync(int contaConjuntaId, int usuarioId);

        /// <summary>
        /// Atualiza as permissões de um usuário na conta conjunta
        /// </summary>
        Task AtualizarPermissoesUsuarioAsync(int contaConjuntaId, int usuarioId, 
            bool? podeLer = null, bool? podeEscrever = null, bool? podeAdministrar = null);

        /// <summary>
        /// Atualiza o percentual de participação de um usuário
        /// </summary>
        Task AtualizarPercentualParticipacaoAsync(int contaConjuntaId, int usuarioId, decimal novoPercentual);

        /// <summary>
        /// Obtém o histórico de apurações da conta conjunta
        /// </summary>
        Task<List<ApuracaoResult>> ObterHistoricoApuracoesAsync(int contaConjuntaId, int? ano = null);

        /// <summary>
        /// Calcula a projeção de rateio para um determinado mês
        /// </summary>
        Task<Dictionary<int, decimal>> CalcularProjecaoRateioAsync(int contaConjuntaId, int ano, int mes);
    }

    /// <summary>
    /// Tipos de permissão para contas conjuntas
    /// </summary>
    public enum TipoPermissao
    {
        Leitura = 1,
        Escrita = 2,
        Administracao = 3
    }
}
