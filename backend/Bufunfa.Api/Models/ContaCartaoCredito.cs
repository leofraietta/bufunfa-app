using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Conta Cartão de Crédito - possui regras específicas de fechamento e vencimento
    /// Pode estar vinculada a uma Conta Corrente responsável pelo pagamento
    /// </summary>
    public class ContaCartaoCredito : Conta
    {
        public ContaCartaoCredito()
        {
            Tipo = TipoConta.ContaCartaoCredito;
        }

        /// <summary>
        /// Dia do mês em que a fatura fecha (1-31)
        /// </summary>
        [Required]
        [Range(1, 31)]
        public int DiaFechamento { get; set; }

        /// <summary>
        /// Dia do mês em que a fatura vence (1-31)
        /// </summary>
        [Required]
        [Range(1, 31)]
        public int DiaVencimento { get; set; }

        /// <summary>
        /// Limite de crédito do cartão
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LimiteCredito { get; set; }

        /// <summary>
        /// Taxa de juros rotativo (% ao mês)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxaJurosRotativo { get; set; }

        /// <summary>
        /// Taxa de multa por atraso (% sobre o valor em atraso)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxaMultaAtraso { get; set; }

        /// <summary>
        /// Número do cartão (últimos 4 dígitos para segurança)
        /// </summary>
        [MaxLength(4)]
        public string? UltimosDigitos { get; set; }

        /// <summary>
        /// Nome impresso no cartão
        /// </summary>
        [MaxLength(255)]
        public string? NomeImpresso { get; set; }

        /// <summary>
        /// Bandeira do cartão (Visa, Mastercard, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Bandeira { get; set; }

        /// <summary>
        /// ID da conta corrente responsável pelo pagamento da fatura
        /// </summary>
        public int? ContaCorrenteResponsavelId { get; set; }

        /// <summary>
        /// Conta corrente responsável pelo pagamento da fatura
        /// </summary>
        [JsonIgnore]
        public ContaCorrente? ContaCorrenteResponsavel { get; set; }

        /// <summary>
        /// Calcula o limite disponível
        /// </summary>
        public decimal CalcularLimiteDisponivel()
        {
            return LimiteCredito + SaldoAtual; // SaldoAtual é negativo para cartão de crédito
        }

        /// <summary>
        /// Verifica se a data está após o fechamento da fatura do mês
        /// </summary>
        public bool DataAposFechamento(DateTime data)
        {
            var dataFechamento = new DateTime(data.Year, data.Month, DiaFechamento);
            
            // Se o dia de fechamento é maior que o último dia do mês, usa o último dia
            if (DiaFechamento > DateTime.DaysInMonth(data.Year, data.Month))
            {
                dataFechamento = new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month));
            }

            return data.Date > dataFechamento.Date;
        }

        /// <summary>
        /// Calcula a data de vencimento da fatura para um determinado mês/ano
        /// </summary>
        public DateTime CalcularDataVencimento(int ano, int mes)
        {
            var dataVencimento = new DateTime(ano, mes, DiaVencimento);
            
            // Se o dia de vencimento é maior que o último dia do mês, usa o último dia
            if (DiaVencimento > DateTime.DaysInMonth(ano, mes))
            {
                dataVencimento = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));
            }

            return dataVencimento;
        }

        /// <summary>
        /// Verifica se pode receber lançamento considerando data de fechamento e limite
        /// </summary>
        public override bool PodeReceberLancamento(Lancamento lancamento)
        {
            if (!base.PodeReceberLancamento(lancamento))
                return false;

            // Verifica se a data do lançamento não está após o fechamento da fatura
            if (DataAposFechamento(lancamento.DataInicial))
            {
                return false; // Não pode lançar após fechamento da fatura
            }

            // Se é uma despesa, verifica se não vai ultrapassar o limite
            if (lancamento.Tipo == TipoLancamento.Despesa)
            {
                var limiteDisponivel = CalcularLimiteDisponivel();
                return lancamento.Valor <= limiteDisponivel;
            }

            return true;
        }

        /// <summary>
        /// Calcula o valor da fatura para um determinado mês/ano
        /// </summary>
        public decimal CalcularValorFatura(int ano, int mes)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));
            
            // Ajusta as datas considerando o ciclo de fechamento
            var dataFechamentoAnterior = new DateTime(ano, mes, DiaFechamento).AddMonths(-1);
            var dataFechamentoAtual = new DateTime(ano, mes, DiaFechamento);

            return Lancamentos
                .Where(l => l.DataInicial > dataFechamentoAnterior && l.DataInicial <= dataFechamentoAtual)
                .Where(l => l.Tipo == TipoLancamento.Despesa)
                .Sum(l => l.Valor);
        }
    }
}
