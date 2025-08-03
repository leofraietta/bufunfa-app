using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public class ContaConjunta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoAtual { get; set; }

        [Required]
        public DateTime DataApuracao { get; set; }

        // Configuração para saldo positivo
        public bool ManterSaldoPositivo { get; set; } = true;

        // Chave estrangeira para o usuário criador
        [Required]
        public int UsuarioCriadorId { get; set; }
        public Usuario UsuarioCriador { get; set; }

        // Relacionamento com rateios
        public ICollection<Rateio> Rateios { get; set; } = new List<Rateio>();
    }
}

