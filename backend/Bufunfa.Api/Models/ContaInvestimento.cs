using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Tipos de investimento disponíveis
    /// </summary>
    public enum TipoInvestimento
    {
        RendaFixa = 1,
        RendaVariavel = 2,
        FundoInvestimento = 3,
        Previdencia = 4,
        Criptomoeda = 5,
        Outro = 99
    }

    /// <summary>
    /// Conta Investimento - para controle de investimentos e aplicações
    /// </summary>
    public class ContaInvestimento : Conta
    {
        public ContaInvestimento()
        {
            Tipo = TipoConta.ContaInvestimento;
        }

        /// <summary>
        /// Tipo de investimento
        /// </summary>
        [Required]
        public TipoInvestimento TipoInvestimento { get; set; }

        /// <summary>
        /// Nome do produto/ativo de investimento
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string NomeProduto { get; set; }

        /// <summary>
        /// Instituição financeira responsável
        /// </summary>
        [MaxLength(255)]
        public string? InstituicaoFinanceira { get; set; }

        /// <summary>
        /// Taxa de rentabilidade esperada (% ao ano)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxaRentabilidadeEsperada { get; set; }

        /// <summary>
        /// Data de vencimento do investimento (se aplicável)
        /// </summary>
        public DateTime? DataVencimento { get; set; }

        /// <summary>
        /// Valor investido inicialmente
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorInvestidoInicial { get; set; }

        /// <summary>
        /// Valor atual do investimento (com rendimentos)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorAtualInvestimento { get; set; }

        /// <summary>
        /// Data da última atualização do valor
        /// </summary>
        public DateTime? DataUltimaAtualizacaoValor { get; set; }

        /// <summary>
        /// Indica se o investimento permite resgates parciais
        /// </summary>
        public bool PermiteResgateParcial { get; set; } = true;

        /// <summary>
        /// Valor mínimo para resgate (se aplicável)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorMinimoResgate { get; set; }

        /// <summary>
        /// Prazo de carência para resgate (em dias)
        /// </summary>
        public int? PrazoCarenciaDias { get; set; }

        /// <summary>
        /// Código do ativo (para ações, fundos, etc.)
        /// </summary>
        [MaxLength(20)]
        public string? CodigoAtivo { get; set; }

        /// <summary>
        /// Calcula o rendimento atual do investimento
        /// </summary>
        public decimal CalcularRendimento()
        {
            return ValorAtualInvestimento - ValorInvestidoInicial;
        }

        /// <summary>
        /// Calcula a rentabilidade percentual atual
        /// </summary>
        public decimal CalcularRentabilidadePercentual()
        {
            if (ValorInvestidoInicial == 0)
                return 0;

            return ((ValorAtualInvestimento - ValorInvestidoInicial) / ValorInvestidoInicial) * 100;
        }

        /// <summary>
        /// Verifica se o investimento está no prazo de carência
        /// </summary>
        public bool EstaEmCarencia()
        {
            if (!PrazoCarenciaDias.HasValue)
                return false;

            var dataFimCarencia = DataCriacao.AddDays(PrazoCarenciaDias.Value);
            return DateTime.UtcNow < dataFimCarencia;
        }

        /// <summary>
        /// Verifica se pode receber lançamento considerando regras de investimento
        /// </summary>
        public override bool PodeReceberLancamento(Lancamento lancamento)
        {
            if (!base.PodeReceberLancamento(lancamento))
                return false;

            // Se é um resgate (despesa), verifica as regras específicas
            if (lancamento.Tipo == TipoLancamento.Despesa)
            {
                // Verifica carência
                if (EstaEmCarencia())
                    return false;

                // Verifica se permite resgate parcial
                if (!PermiteResgateParcial && lancamento.ValorReal < ValorAtualInvestimento)
                    return false;

                // Verifica valor mínimo de resgate
                if (ValorMinimoResgate.HasValue && lancamento.ValorReal < ValorMinimoResgate.Value)
                    return false;

                // Verifica se não vai resgatar mais do que tem
                if (lancamento.ValorReal > ValorAtualInvestimento)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calcula o valor projetado do investimento em uma data futura
        /// </summary>
        public decimal CalcularValorProjetado(DateTime dataFutura)
        {
            if (!TaxaRentabilidadeEsperada.HasValue || dataFutura <= DateTime.UtcNow)
                return ValorAtualInvestimento;

            var diasInvestimento = (dataFutura - DateTime.UtcNow).Days;
            var taxaDiaria = (TaxaRentabilidadeEsperada.Value / 100) / 365;
            
            return ValorAtualInvestimento * (decimal)Math.Pow((double)(1 + taxaDiaria), diasInvestimento);
        }

        /// <summary>
        /// Atualiza o valor atual do investimento
        /// </summary>
        public void AtualizarValorInvestimento(decimal novoValor)
        {
            ValorAtualInvestimento = novoValor;
            SaldoAtual = novoValor; // Sincroniza com o saldo da conta base
            DataUltimaAtualizacaoValor = DateTime.UtcNow;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
