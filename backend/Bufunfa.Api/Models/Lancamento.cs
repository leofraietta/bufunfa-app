using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

    public enum StatusLancamento
    {
        Provisional = 1,
        Realizado = 2,
        Cancelado = 3,
        Quitado = 4
    }

    public enum TipoPeriodicidade
    {
        Semanal = 1,
        Quinzenal = 2,
        Mensal = 3,
        Bimestral = 4,
        Trimestral = 5,
        Semestral = 6,
        Anual = 7,
        TodoDiaUtil = 8,
        Personalizado = 9
    }

    public abstract class Lancamento
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

        public TipoLancamento Tipo { get; set; }

        [Required]
        public TipoRecorrencia TipoRecorrencia { get; set; }

        [Required]
        public StatusLancamento Status { get; set; } = StatusLancamento.Provisional;

        // Data final para lançamentos recorrentes/parcelados/periódicos (opcional)
        public DateTime? DataFinal { get; set; }

        // Propriedades específicas para diferentes tipos de lançamento
        // Para lançamentos parcelados
        public int? QuantidadeParcelas { get; set; }
        public int? ParcelaAtual { get; set; }

        // Para lançamentos recorrentes - dia do mês (1-31)
        public int? DiaVencimento { get; set; }

        // Para lançamentos recorrentes com padrões específicos
        public TipoPeriodicidade? TipoPeriodicidade { get; set; }
        public int? IntervaloDias { get; set; } // Para periodicidade personalizada (1-6 dias)
        public bool AjustarDiaUtil { get; set; } = true; // Ajustar para próximo dia útil em finais de semana

        // Controle de processamento
        public bool ProcessarRetroativo { get; set; } = false;
        public DateTime? UltimaDataProcessamento { get; set; }

        // Chaves estrangeiras
        [Required]
        public int ContaId { get; set; }
        [JsonIgnore]
        public Conta Conta { get; set; }

        public int? CategoriaId { get; set; }
        [JsonIgnore]
        public Categoria Categoria { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        [JsonIgnore]
        public Usuario Usuario { get; set; }

        // Controle
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; } = true;

        // Relacionamento com lançamentos de folha
        [JsonIgnore]
        public ICollection<LancamentoFolha> LancamentosFolha { get; set; } = new List<LancamentoFolha>();

        // Propriedades calculadas
        [NotMapped]
        public decimal Valor => ValorReal ?? ValorProvisionado;

        [NotMapped]
        public DateTime Data => DataInicial;

        [NotMapped]
        public bool EhRealizado => Status == StatusLancamento.Realizado;

        [NotMapped]
        public bool EhCancelado => Status == StatusLancamento.Cancelado;

        [NotMapped]
        public bool EhQuitado => Status == StatusLancamento.Quitado;

        // Métodos abstratos para implementação nas classes especializadas
        public abstract bool PodeSerProcessadoEm(DateTime data);
        public abstract DateTime? ProximaDataVencimento(DateTime? dataReferencia = null);
        public abstract IEnumerable<DateTime> ObterDatasVencimento(DateTime dataInicio, DateTime dataFim);
        public abstract bool EhValido();

        // Métodos virtuais que podem ser sobrescritos
        public virtual void CalcularDataFinal()
        {
            // Implementação padrão - pode ser sobrescrita nas classes filhas
        }

        public virtual string ObterDescricaoCompleta()
        {
            return $"{Descricao} - {Tipo} ({TipoRecorrencia})";
        }
    }
}
