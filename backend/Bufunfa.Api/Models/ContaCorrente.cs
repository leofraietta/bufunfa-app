using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Conta Corrente - tipo mais comum de conta bancária
    /// Pode ser responsável por contas de cartão de crédito
    /// </summary>
    public class ContaCorrente : Conta
    {
        public ContaCorrente()
        {
            Tipo = TipoConta.ContaCorrente;
        }

        /// <summary>
        /// Limite de cheque especial (se aplicável)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LimiteChequeEspecial { get; set; }

        /// <summary>
        /// Taxa de juros do cheque especial (% ao mês)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxaJurosChequeEspecial { get; set; }

        /// <summary>
        /// Número da conta (opcional)
        /// </summary>
        [MaxLength(50)]
        public string? NumeroConta { get; set; }

        /// <summary>
        /// Número da agência (opcional)
        /// </summary>
        [MaxLength(20)]
        public string? NumeroAgencia { get; set; }

        /// <summary>
        /// Nome do banco (opcional)
        /// </summary>
        [MaxLength(255)]
        public string? NomeBanco { get; set; }

        /// <summary>
        /// Relacionamento com contas de cartão de crédito que esta conta é responsável
        /// </summary>
        public ICollection<ContaCartaoCredito> CartoesCredito { get; set; } = new List<ContaCartaoCredito>();

        /// <summary>
        /// Calcula o saldo considerando o limite do cheque especial
        /// </summary>
        public override decimal CalcularSaldo()
        {
            var saldoBase = base.CalcularSaldo();
            if (LimiteChequeEspecial.HasValue && saldoBase < 0)
            {
                // Se está no vermelho, considera o limite do cheque especial
                return saldoBase + LimiteChequeEspecial.Value;
            }
            return saldoBase;
        }

        /// <summary>
        /// Verifica se pode receber lançamento considerando limite do cheque especial
        /// </summary>
        public override bool PodeReceberLancamento(Lancamento lancamento)
        {
            if (!base.PodeReceberLancamento(lancamento))
                return false;

            // Se é uma despesa, verifica se não vai ultrapassar o limite
            if (lancamento.Tipo == TipoLancamento.Despesa)
            {
                var saldoAposLancamento = SaldoAtual - lancamento.Valor;
                var limiteTotal = LimiteChequeEspecial ?? 0;
                return saldoAposLancamento >= -limiteTotal;
            }

            return true;
        }
    }
}
