using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public class LancamentoFolha
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FolhaMensalId { get; set; }
        public FolhaMensal FolhaMensal { get; set; }

        [Required]
        public int LancamentoOrigemId { get; set; }
        public Lancamento LancamentoOrigem { get; set; }

        [Required]
        [MaxLength(255)]
        public string Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorProvisionado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorReal { get; set; }

        [Required]
        public DateTime DataPrevista { get; set; }

        public DateTime? DataRealizacao { get; set; }

        [Required]
        public TipoLancamento Tipo { get; set; }

        [Required]
        public TipoRecorrencia TipoRecorrencia { get; set; }

        // Para lançamentos parcelados
        public int? ParcelaAtual { get; set; }
        public int? TotalParcelas { get; set; }

        public int? CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public bool Realizado { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Propriedades calculadas
        public decimal ValorEfetivo => ValorReal ?? ValorProvisionado;
        public bool EmAtraso => !Realizado && DataPrevista < DateTime.UtcNow.Date;
        public string StatusDescricao => Realizado ? "Realizado" : (EmAtraso ? "Em Atraso" : "Pendente");
    }
}

