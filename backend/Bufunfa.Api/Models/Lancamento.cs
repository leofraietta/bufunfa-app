using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public enum TipoLancamento
    {
        Receita = 1,
        Despesa = 2
    }

    public enum TipoRecorrencia
    {
        Esporadico = 1,
        Recorrente = 2,
        Parcelado = 3
    }

    public class Lancamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorProvisionado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorReal { get; set; }

        [Required]
        public DateTime DataInicial { get; set; }

        [Required]
        public TipoLancamento Tipo { get; set; }

        [Required]
        public TipoRecorrencia TipoRecorrencia { get; set; }

        // Para lançamentos parcelados
        public int? QuantidadeParcelas { get; set; }

        // Para lançamentos recorrentes - dia do mês (1-31)
        public int? DiaVencimento { get; set; }

        // Data final para lançamentos recorrentes (opcional)
        public DateTime? DataFinal { get; set; }

        // Chaves estrangeiras
        [Required]
        public int ContaId { get; set; }
        public Conta Conta { get; set; }

        public int? CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // Controle
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; } = true;

        // Relacionamento com lançamentos de folha
        public ICollection<LancamentoFolha> LancamentosFolha { get; set; } = new List<LancamentoFolha>();

        // Propriedades calculadas
        [NotMapped]
        public decimal Valor => ValorReal ?? ValorProvisionado;

        [NotMapped]
        public DateTime Data => DataInicial;

        [NotMapped]
        public int? ParcelaAtual => null; // Será calculado dinamicamente para cada folha
    }
}

