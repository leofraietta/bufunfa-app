using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bufunfa.Api.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Valor provisionado mensal para esta categoria
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorProvisionadoMensal { get; set; } = 0;

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        // Chave estrangeira para o usuário
        public int UsuarioId { get; set; }
        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        // Relacionamento com lançamentos
        [JsonIgnore]
        public ICollection<Lancamento> Lancamentos { get; set; } = new List<Lancamento>();

        /// <summary>
        /// Calcula o gasto real da categoria em um mês específico
        /// </summary>
        public decimal CalcularGastoReal(int ano, int mes)
        {
            return Lancamentos?
                .Where(l => l.Status == StatusLancamento.Realizado &&
                           l.DataInicial.Year == ano &&
                           l.DataInicial.Month == mes)
                .Sum(l => l.ValorReal ?? l.ValorProvisionado) ?? 0;
        }

        /// <summary>
        /// Calcula o saldo da categoria (provisionado - gasto real) em um mês
        /// </summary>
        public decimal CalcularSaldo(int ano, int mes)
        {
            return ValorProvisionadoMensal - CalcularGastoReal(ano, mes);
        }
    }
}

