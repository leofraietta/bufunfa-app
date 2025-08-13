using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Conta Poupança - conta com rendimento automático e regras específicas
    /// </summary>
    public class ContaPoupanca : Conta
    {
        public ContaPoupanca()
        {
            Tipo = TipoConta.ContaPoupanca;
        }

        /// <summary>
        /// Taxa de rendimento mensal (% ao mês)
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal? TaxaRendimentoMensal { get; set; }

        /// <summary>
        /// Dia do mês em que o rendimento é creditado (aniversário da conta)
        /// </summary>
        [Range(1, 31)]
        public int? DiaAniversario { get; set; }

        /// <summary>
        /// Valor mínimo para manter na conta
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorMinimoManutencao { get; set; }

        /// <summary>
        /// Número da conta poupança (opcional)
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
        /// Calcula o rendimento mensal baseado no saldo e taxa
        /// </summary>
        public decimal CalcularRendimentoMensal()
        {
            if (!TaxaRendimentoMensal.HasValue || SaldoAtual <= 0)
                return 0;

            return SaldoAtual * (TaxaRendimentoMensal.Value / 100);
        }

        /// <summary>
        /// Calcula a data do próximo rendimento
        /// </summary>
        public DateTime? CalcularProximoRendimento()
        {
            if (!DiaAniversario.HasValue)
                return null;

            var hoje = DateTime.Today;
            var proximoRendimento = new DateTime(hoje.Year, hoje.Month, DiaAniversario.Value);

            // Se já passou do dia do aniversário neste mês, vai para o próximo mês
            if (proximoRendimento <= hoje)
            {
                proximoRendimento = proximoRendimento.AddMonths(1);
            }

            // Ajusta se o dia não existe no mês (ex: 31 de fevereiro)
            if (DiaAniversario.Value > DateTime.DaysInMonth(proximoRendimento.Year, proximoRendimento.Month))
            {
                proximoRendimento = new DateTime(proximoRendimento.Year, proximoRendimento.Month, 
                    DateTime.DaysInMonth(proximoRendimento.Year, proximoRendimento.Month));
            }

            return proximoRendimento;
        }

        /// <summary>
        /// Verifica se pode receber lançamento considerando valor mínimo
        /// </summary>
        public override bool PodeReceberLancamento(Lancamento lancamento)
        {
            if (!base.PodeReceberLancamento(lancamento))
                return false;

            // Se é uma despesa/saque, verifica se não vai ficar abaixo do valor mínimo
            if (lancamento.Tipo == TipoLancamento.Despesa && ValorMinimoManutencao.HasValue)
            {
                var saldoAposLancamento = SaldoAtual - lancamento.ValorReal;
                return saldoAposLancamento >= ValorMinimoManutencao.Value;
            }

            return true;
        }

        /// <summary>
        /// Calcula o saldo projetado com rendimentos futuros
        /// </summary>
        public decimal CalcularSaldoProjetado(int mesesFuturos)
        {
            if (!TaxaRendimentoMensal.HasValue || mesesFuturos <= 0)
                return SaldoAtual;

            var saldoProjetado = SaldoAtual;
            var taxaMensal = TaxaRendimentoMensal.Value / 100;

            for (int i = 0; i < mesesFuturos; i++)
            {
                saldoProjetado += saldoProjetado * taxaMensal;
            }

            return saldoProjetado;
        }
    }
}
