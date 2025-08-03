using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public enum TipoConta
    {
        Principal = 1,
        CartaoCredito = 2
    }

    public class Conta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Required]
        public TipoConta Tipo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicial { get; set; }

        // Propriedades específicas para cartão de crédito
        public DateTime? DataFechamento { get; set; }
        public DateTime? DataVencimento { get; set; }

        // Chave estrangeira para o usuário
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // Relacionamento com lançamentos
        public ICollection<Lancamento> Lancamentos { get; set; } = new List<Lancamento>();
    }
}

