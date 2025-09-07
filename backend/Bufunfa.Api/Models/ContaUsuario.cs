using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public enum PermissionLevel
    {
        ViewOnly = 1,
        FullAccess = 2
    }
    /// <summary>
    /// Tabela de relacionamento many-to-many entre Conta e Usuario
    /// Permite que uma conta tenha múltiplos usuários e um usuário tenha múltiplas contas
    /// </summary>
    public class ContaUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaId { get; set; }
        public Conta Conta { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        /// <summary>
        /// Indica se o usuário é o proprietário principal da conta
        /// </summary>
        public bool EhProprietario { get; set; } = false;

        /// <summary>
        /// Percentual de participação do usuário na conta (usado principalmente em contas conjuntas)
        /// </summary>
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentualParticipacao { get; set; } = 100.00m;

        /// <summary>
        /// Indica se o usuário é administrador da conta (para contas conjuntas)
        /// </summary>
        public bool EhAdministrador { get; set; } = false;

        /// <summary>
        /// Nível de permissão do usuário na conta
        /// </summary>
        public PermissionLevel NivelPermissao { get; set; } = PermissionLevel.FullAccess;

        /// <summary>
        /// ID do usuário que convidou este usuário (para contas conjuntas)
        /// </summary>
        public int? ConvidadoPorUsuarioId { get; set; }
        public Usuario ConvidadoPorUsuario { get; set; }

        /// <summary>
        /// Data do convite
        /// </summary>
        public DateTime? DataConvite { get; set; }

        public DateTime DataVinculacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataDesvinculacao { get; set; }
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Verifica se o usuário pode ler dados da conta
        /// </summary>
        public bool PodeLer => Ativo && (NivelPermissao == PermissionLevel.ViewOnly || NivelPermissao == PermissionLevel.FullAccess);

        /// <summary>
        /// Verifica se o usuário pode escrever/modificar dados da conta
        /// </summary>
        public bool PodeEscrever => Ativo && NivelPermissao == PermissionLevel.FullAccess;

        /// <summary>
        /// Verifica se o usuário pode administrar a conta (convidar usuários, alterar permissões, etc.)
        /// </summary>
        public bool PodeAdministrar => Ativo && EhAdministrador;
    }
}
