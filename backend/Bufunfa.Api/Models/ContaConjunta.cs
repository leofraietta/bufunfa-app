using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Conta Conjunta - compartilhada entre múltiplos usuários com rateio configurável
    /// Herda de Conta e implementa lógicas específicas de rateio e apuração
    /// </summary>
    public class ContaConjunta : Conta
    {
        public ContaConjunta()
        {
            Tipo = TipoConta.ContaConjunta;
        }

        /// <summary>
        /// Data de apuração mensal da conta conjunta (sempre último dia do mês conforme requisitos)
        /// </summary>
        [Required]
        public int DiaApuracao { get; set; } = 31;

        /// <summary>
        /// Configuração para saldo positivo:
        /// true = manter saldo na conta conjunta
        /// false = distribuir como receita nas contas principais
        /// </summary>
        public bool ManterSaldoPositivo { get; set; } = true;

        /// <summary>
        /// Indica se a conta está em processo de apuração
        /// </summary>
        public bool EmApuracao { get; set; } = false;

        /// <summary>
        /// Data da última apuração realizada
        /// </summary>
        public DateTime? DataUltimaApuracao { get; set; }

        /// <summary>
        /// Valor da última apuração
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorUltimaApuracao { get; set; }

        /// <summary>
        /// Calcula o rateio do saldo entre os usuários participantes
        /// </summary>
        public Dictionary<int, decimal> CalcularRateioSaldo()
        {
            var rateio = new Dictionary<int, decimal>();
            var participantes = ContaUsuarios.Where(cu => cu.Ativo).ToList();
            
            if (!participantes.Any())
                return rateio;

            foreach (var participante in participantes)
            {
                var valorRateado = SaldoAtual * (participante.PercentualParticipacao / 100);
                rateio[participante.UsuarioId] = valorRateado;
            }

            return rateio;
        }

        /// <summary>
        /// Verifica se todos os percentuais de participação somam 100%
        /// </summary>
        public bool ValidarPercentuaisRateio()
        {
            var totalPercentual = ContaUsuarios
                .Where(cu => cu.Ativo)
                .Sum(cu => cu.PercentualParticipacao);
            
            return Math.Abs(totalPercentual - 100) < 0.01m; // Tolerância para arredondamento
        }

        /// <summary>
        /// Calcula a data de apuração para um determinado mês/ano (sempre último dia do mês)
        /// </summary>
        public DateTime CalcularDataApuracao(int ano, int mes)
        {
            // Conforme requisitos refinados: sempre último dia do mês
            return new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));
        }

        /// <summary>
        /// Verifica se pode receber lançamento considerando regras da conta conjunta
        /// </summary>
        public override bool PodeReceberLancamento(Lancamento lancamento)
        {
            if (!base.PodeReceberLancamento(lancamento))
                return false;

            // Verifica se a conta não está em processo de apuração
            if (EmApuracao)
                return false;

            // Verifica se os percentuais de rateio estão válidos
            if (!ValidarPercentuaisRateio())
                return false;

            return true;
        }

        /// <summary>
        /// Processa a apuração mensal da conta conjunta
        /// </summary>
        public ApuracaoResult ProcessarApuracao(int ano, int mes)
        {
            var result = new ApuracaoResult
            {
                Ano = ano,
                Mes = mes,
                DataApuracao = CalcularDataApuracao(ano, mes),
                SaldoApurado = SaldoAtual,
                Rateios = CalcularRateioSaldo()
            };

            // Marca como em apuração
            EmApuracao = true;
            DataUltimaApuracao = DateTime.UtcNow;
            ValorUltimaApuracao = SaldoAtual;

            return result;
        }

        // Propriedades para compatibilidade com código existente
        
        /// <summary>
        /// ID do usuário administrador da conta
        /// </summary>
        [NotMapped]
        public int? UsuarioAdministradorId => ContaUsuarios?.FirstOrDefault(cu => cu.EhAdministrador && cu.Ativo)?.UsuarioId;

        /// <summary>
        /// Usuário administrador da conta
        /// </summary>
        [NotMapped]
        public Usuario? UsuarioAdministrador => ContaUsuarios?.FirstOrDefault(cu => cu.EhAdministrador && cu.Ativo)?.Usuario;

        /// <summary>
        /// ID do usuário criador da conta (para compatibilidade)
        /// </summary>
        [NotMapped]
        public int? UsuarioCriadorId => UsuarioAdministradorId;

        /// <summary>
        /// Usuário criador da conta (para compatibilidade)
        /// </summary>
        [NotMapped]
        public Usuario? UsuarioCriador => UsuarioAdministrador;

        /// <summary>
        /// Simulação da propriedade Rateios para compatibilidade com código existente
        /// </summary>
        [NotMapped]
        public IEnumerable<RateioCompatibilidade> Rateios => 
            ContaUsuarios?.Where(cu => cu.Ativo)
                         .Select(cu => new RateioCompatibilidade
                         {
                             Id = cu.Id,
                             PercentualRateio = cu.PercentualParticipacao,
                             UsuarioId = cu.UsuarioId,
                             Usuario = cu.Usuario,
                             ContaConjuntaId = Id,
                             ContaConjunta = this
                         }) ?? Enumerable.Empty<RateioCompatibilidade>();
    }

    /// <summary>
    /// Resultado do processo de apuração da conta conjunta
    /// </summary>
    public class ApuracaoResult
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public DateTime DataApuracao { get; set; }
        public decimal SaldoApurado { get; set; }
        public Dictionary<int, decimal> Rateios { get; set; } = new Dictionary<int, decimal>();
        public bool SaldoPositivo => SaldoApurado > 0;
        public bool SaldoNegativo => SaldoApurado < 0;
    }
}
