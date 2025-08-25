using Bufunfa.Api.Models;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Interface para gerenciamento automático de folhas mensais
    /// Responsável pela criação automática de folhas quando contas são criadas
    /// e pela projeção de folhas futuras
    /// </summary>
    public interface IFolhaAutomaticaService
    {
        /// <summary>
        /// Cria automaticamente a folha do mês atual quando uma nova conta é criada
        /// </summary>
        Task CriarFolhaMensalInicialAsync(Conta conta, int usuarioId);

        /// <summary>
        /// Cria folhas futuras para projeção financeira
        /// </summary>
        Task<FolhaMensal> CriarFolhaFuturaAsync(int usuarioId, int contaId, int ano, int mes);

        /// <summary>
        /// Verifica e cria folhas automáticas para lançamentos recorrentes/parcelados
        /// que devem aparecer em meses futuros
        /// </summary>
        Task ProcessarFolhasFuturasAutomaticasAsync(int usuarioId, int contaId, int mesesAFrente = 12);

        /// <summary>
        /// Atualiza todas as folhas futuras quando um lançamento recorrente/parcelado é alterado
        /// </summary>
        Task AtualizarFolhasFuturasAsync(Lancamento lancamento);
    }
}
