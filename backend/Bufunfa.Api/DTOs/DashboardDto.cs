namespace Bufunfa.Api.DTOs
{
    public class DashboardDto
    {
        public decimal SaldoTotal { get; set; }
        public decimal ReceitasMensais { get; set; }
        public decimal DespesasMensais { get; set; }
        public decimal ProjecaoSaldo { get; set; }
        public List<LancamentoResumoDto> ProximosVencimentos { get; set; } = new();
    }

    public class LancamentoResumoDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal? ValorProvisionado { get; set; }
        public DateTime DataInicial { get; set; }
        public int Tipo { get; set; }
        public int TipoRecorrencia { get; set; }
        public int ContaId { get; set; }
        public bool Realizado { get; set; }
        public bool Cancelado { get; set; }
        public bool Quitado { get; set; }
    }
}
