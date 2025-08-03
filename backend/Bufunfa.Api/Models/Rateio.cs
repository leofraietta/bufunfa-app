using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public class Rateio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentualRateio { get; set; }

        // Chaves estrangeiras
        [Required]
        public int ContaConjuntaId { get; set; }
        public ContaConjunta ContaConjunta { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}

