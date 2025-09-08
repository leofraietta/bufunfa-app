using System.ComponentModel.DataAnnotations;
using Bufunfa.Api.Models;

namespace Bufunfa.Api.DTOs
{
    /// <summary>
    /// DTO para atualização de lançamentos - evita problemas de deserialização com classes abstratas
    /// </summary>
    public class AtualizarLancamentoDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public decimal ValorProvisionado { get; set; }

        public decimal? ValorReal { get; set; }

        [Required]
        public DateTime DataInicial { get; set; }

        [Required]
        public TipoLancamento Tipo { get; set; }

        [Required]
        public TipoRecorrencia TipoRecorrencia { get; set; }

        public DateTime? DataFinal { get; set; }

        // Propriedades específicas para diferentes tipos
        public int? QuantidadeParcelas { get; set; }
        public int? ParcelaAtual { get; set; }
        public int? DiaVencimento { get; set; }
        public TipoPeriodicidade? TipoPeriodicidade { get; set; }
        public int? IntervaloDias { get; set; }
        public bool AjustarDiaUtil { get; set; }

        public bool ProcessarRetroativo { get; set; } = false;

        [Required]
        public int ContaId { get; set; }

        public int? CategoriaId { get; set; }

        public StatusLancamento Status { get; set; }

        /// <summary>
        /// Aplica as alterações do DTO ao lançamento existente
        /// </summary>
        public void AplicarAlteracoes(Lancamento lancamento)
        {
            lancamento.Descricao = Descricao;
            lancamento.ValorProvisionado = ValorProvisionado;
            lancamento.ValorReal = ValorReal;
            lancamento.DataInicial = DataInicial;
            lancamento.Tipo = Tipo;
            lancamento.ContaId = ContaId;
            lancamento.CategoriaId = CategoriaId;
            lancamento.Status = Status;
            lancamento.DataFinal = DataFinal;

            // Aplicar propriedades específicas baseadas no tipo
            switch (lancamento)
            {
                case LancamentoRecorrente recorrente:
                    recorrente.TipoPeriodicidade = TipoPeriodicidade;
                    recorrente.IntervaloDias = IntervaloDias;
                    recorrente.AjustarDiaUtil = AjustarDiaUtil;
                    recorrente.DiaVencimento = DiaVencimento ?? recorrente.DiaVencimento;
                    recorrente.ProcessarRetroativo = ProcessarRetroativo;
                    break;

                case LancamentoParcelado parcelado:
                    parcelado.QuantidadeParcelas = QuantidadeParcelas ?? parcelado.QuantidadeParcelas;
                    parcelado.ParcelaAtual = ParcelaAtual ?? parcelado.ParcelaAtual;
                    parcelado.ProcessarRetroativo = ProcessarRetroativo;
                    break;

                case LancamentoEsporadico esporadico:
                    // Lançamentos esporádicos não têm propriedades específicas adicionais
                    break;
            }
        }
    }
}
