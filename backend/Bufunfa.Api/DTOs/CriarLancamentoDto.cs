using System.ComponentModel.DataAnnotations;
using Bufunfa.Api.Models;

namespace Bufunfa.Api.DTOs
{
    /// <summary>
    /// DTO para criação de lançamentos - evita problemas de deserialização com classes abstratas
    /// </summary>
    public class CriarLancamentoDto
    {
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

        public bool ProcessarRetroativo { get; set; } = false;

        [Required]
        public int ContaId { get; set; }

        public int? CategoriaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Lancamento apropriada
        /// </summary>
        public Lancamento ToLancamento()
        {
            return TipoRecorrencia switch
            {
                TipoRecorrencia.Esporadico => new LancamentoEsporadico
                {
                    Descricao = Descricao,
                    ValorProvisionado = ValorProvisionado,
                    ValorReal = ValorReal,
                    DataInicial = DataInicial,
                    Tipo = Tipo,
                    TipoRecorrencia = TipoRecorrencia,
                    ContaId = ContaId,
                    CategoriaId = CategoriaId,
                    UsuarioId = UsuarioId,
                    DataCriacao = DateTime.UtcNow
                },
                TipoRecorrencia.Recorrente => new LancamentoRecorrente
                {
                    Descricao = Descricao,
                    ValorProvisionado = ValorProvisionado,
                    ValorReal = ValorReal,
                    DataInicial = DataInicial,
                    Tipo = Tipo,
                    TipoRecorrencia = TipoRecorrencia,
                    DataFinal = DataFinal,
                    DiaVencimento = DiaVencimento ?? 1,
                    ProcessarRetroativo = ProcessarRetroativo,
                    ContaId = ContaId,
                    CategoriaId = CategoriaId,
                    UsuarioId = UsuarioId,
                    DataCriacao = DateTime.UtcNow
                },
                TipoRecorrencia.Parcelado => new LancamentoParcelado
                {
                    Descricao = Descricao,
                    ValorProvisionado = ValorProvisionado,
                    ValorReal = ValorReal,
                    DataInicial = DataInicial,
                    Tipo = Tipo,
                    TipoRecorrencia = TipoRecorrencia,
                    QuantidadeParcelas = QuantidadeParcelas ?? 1,
                    ParcelaAtual = ParcelaAtual ?? 1,
                    ProcessarRetroativo = ProcessarRetroativo,
                    ContaId = ContaId,
                    CategoriaId = CategoriaId,
                    UsuarioId = UsuarioId,
                    DataCriacao = DateTime.UtcNow
                },
                // TipoRecorrencia.Periodico removido - agora é parte de Recorrente
                _ => throw new ArgumentException($"Tipo de recorrência não suportado: {TipoRecorrencia}")
            };
        }
    }
}
