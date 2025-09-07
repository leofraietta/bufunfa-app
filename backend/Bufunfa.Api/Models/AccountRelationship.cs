using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public enum RelationshipType
    {
        PaymentSource = 1,  // Fonte pagadora
        Related = 2         // Conta relacionada
    }

    /// <summary>
    /// Gerencia relacionamentos entre contas (fonte pagadora e contas relacionadas)
    /// Implementa o sistema de relacionamentos definido nos requisitos refinados
    /// </summary>
    public class AccountRelationship
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaOrigemId { get; set; }
        public Conta ContaOrigem { get; set; }

        [Required]
        public int ContaDestinoId { get; set; }
        public Conta ContaDestino { get; set; }

        [Required]
        public RelationshipType TipoRelacionamento { get; set; }

        [NotMapped]
        public RelationshipType RelationshipType => TipoRelacionamento;

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataDesativacao { get; set; }

        [Required]
        public int CriadoPorUsuarioId { get; set; }
        public Usuario CriadoPorUsuario { get; set; }

        // Propriedades calculadas
        [NotMapped]
        public bool EhFontePagadora => RelationshipType == RelationshipType.PaymentSource;

        [NotMapped]
        public bool EhContaRelacionada => RelationshipType == RelationshipType.Related;

        /// <summary>
        /// Desativa o relacionamento
        /// </summary>
        public void Desativar()
        {
            Ativa = false;
            DataDesativacao = DateTime.UtcNow;
        }

        /// <summary>
        /// Reativa o relacionamento
        /// </summary>
        public void Reativar()
        {
            Ativa = true;
            DataDesativacao = null;
        }

        /// <summary>
        /// Valida se o relacionamento é válido
        /// </summary>
        public bool EhValido()
        {
            // Não pode ter relacionamento consigo mesmo
            if (ContaOrigemId == ContaDestinoId) return false;

            // Deve ter contas válidas
            if (ContaOrigemId <= 0 || ContaDestinoId <= 0) return false;

            return true;
        }
    }
}
