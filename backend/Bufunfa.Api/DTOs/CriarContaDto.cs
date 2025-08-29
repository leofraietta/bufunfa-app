using System.ComponentModel.DataAnnotations;
using Bufunfa.Api.Models;

namespace Bufunfa.Api.DTOs
{
    /// <summary>
    /// DTO para criação de contas - evita problemas de deserialização com classes abstratas
    /// </summary>
    public class CriarContaDto
    {
        [Required]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public TipoConta Tipo { get; set; }

        public decimal SaldoInicial { get; set; } = 0;

        // Propriedades específicas para Conta Corrente
        public string? NumeroConta { get; set; }
        public string? NumeroAgencia { get; set; }
        public string? NomeBanco { get; set; }
        public decimal? LimiteChequeEspecial { get; set; }
        public decimal? TaxaJurosChequeEspecial { get; set; }

        // Propriedades específicas para Cartão de Crédito
        public int? DiaFechamento { get; set; }
        public int? DiaVencimento { get; set; }
        public decimal? LimiteCredito { get; set; }
        public decimal? TaxaJurosRotativo { get; set; }
        public decimal? TaxaMultaAtraso { get; set; }
        public string? UltimosDigitos { get; set; }
        public string? NomeImpresso { get; set; }
        public string? Bandeira { get; set; }
        public int? ContaCorrenteResponsavelId { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Conta apropriada
        /// </summary>
        public Conta ToConta()
        {
            return Tipo switch
            {
                TipoConta.ContaCorrente => new ContaCorrente
                {
                    Nome = Nome,
                    Descricao = Descricao,
                    SaldoInicial = SaldoInicial,
                    NumeroConta = NumeroConta,
                    NumeroAgencia = NumeroAgencia,
                    NomeBanco = NomeBanco,
                    LimiteChequeEspecial = LimiteChequeEspecial,
                    TaxaJurosChequeEspecial = TaxaJurosChequeEspecial
                },
                TipoConta.ContaCartaoCredito => new ContaCartaoCredito
                {
                    Nome = Nome,
                    Descricao = Descricao,
                    SaldoInicial = SaldoInicial,
                    DiaFechamento = DiaFechamento ?? 1,
                    DiaVencimento = DiaVencimento ?? 10,
                    LimiteCredito = LimiteCredito ?? 0,
                    TaxaJurosRotativo = TaxaJurosRotativo,
                    TaxaMultaAtraso = TaxaMultaAtraso,
                    UltimosDigitos = UltimosDigitos,
                    NomeImpresso = NomeImpresso,
                    Bandeira = Bandeira,
                    ContaCorrenteResponsavelId = ContaCorrenteResponsavelId
                },
                _ => throw new ArgumentException($"Tipo de conta não suportado: {Tipo}")
            };
        }
    }
}
