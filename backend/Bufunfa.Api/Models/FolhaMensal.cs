using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public class FolhaMensal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public int Mes { get; set; }

        [Required]
        public int ContaId { get; set; }
        public Conta Conta { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicialReal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicialProvisionado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoFinalReal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoFinalProvisionado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceitasReais { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceitasProvisionadas { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDespesasReais { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDespesasProvisionadas { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataFechamento { get; set; }
        public bool Fechada { get; set; }

        // Relacionamento com lançamentos da folha
        public ICollection<LancamentoFolha> LancamentosFolha { get; set; } = new List<LancamentoFolha>();

        // Método para criar chave única por usuário/conta/ano/mês
        public string ChaveUnica => $"{UsuarioId}_{ContaId}_{Ano}_{Mes:D2}";
    }
}

