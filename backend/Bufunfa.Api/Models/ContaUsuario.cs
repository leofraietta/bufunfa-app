using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
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
        /// Permissões do usuário na conta
        /// </summary>
        public bool PodeLer { get; set; } = true;
        public bool PodeEscrever { get; set; } = true;
        public bool PodeAdministrar { get; set; } = false;

        public DateTime DataVinculacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataDesvinculacao { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
