using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bufunfa.Api.Models
{
    public enum TipoConta
    {
        ContaCorrente = 1,
        ContaPoupanca = 2,
        ContaCartaoCredito = 3,
        ContaConjunta = 4,
        ContaInvestimento = 5
    }

    /// <summary>
    /// Classe base abstrata para todos os tipos de conta
    /// Implementa o princípio Open/Closed do SOLID
    /// </summary>
    public abstract class Conta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [MaxLength(500)]
        public string Descricao { get; set; }

        [Required]
        public TipoConta Tipo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicial { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoAtual { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        // Relacionamento many-to-many com usuários
        [JsonIgnore]
        public ICollection<ContaUsuario> ContaUsuarios { get; set; } = new List<ContaUsuario>();

        // Relacionamento com lançamentos
        [JsonIgnore]
        public ICollection<Lancamento> Lancamentos { get; set; } = new List<Lancamento>();

        // Relacionamento com folhas mensais
        [JsonIgnore]
        public ICollection<FolhaMensal> FolhasMensais { get; set; } = new List<FolhaMensal>();

        /// <summary>
        /// Método virtual para calcular saldo - pode ser sobrescrito pelas classes filhas
        /// </summary>
        public virtual decimal CalcularSaldo()
        {
            return SaldoAtual;
        }

        /// <summary>
        /// Método virtual para validar se a conta pode receber um lançamento
        /// </summary>
        public virtual bool PodeReceberLancamento(Lancamento lancamento)
        {
            return Ativa;
        }

        // Métodos auxiliares para compatibilidade com código existente
        
        /// <summary>
        /// Obtém o ID do usuário proprietário principal da conta (para compatibilidade)
        /// </summary>
        [NotMapped]
        public int? UsuarioId => ContaUsuarios?.FirstOrDefault(cu => cu.EhProprietario && cu.Ativo)?.UsuarioId;

        /// <summary>
        /// Obtém o usuário proprietário principal da conta (para compatibilidade)
        /// </summary>
        [NotMapped]
        public Usuario? Usuario => ContaUsuarios?.FirstOrDefault(cu => cu.EhProprietario && cu.Ativo)?.Usuario;

        /// <summary>
        /// Verifica se um usuário é proprietário da conta
        /// </summary>
        public bool EhProprietario(int usuarioId)
        {
            return ContaUsuarios?.Any(cu => cu.UsuarioId == usuarioId && cu.EhProprietario && cu.Ativo) ?? false;
        }

        /// <summary>
        /// Verifica se um usuário tem acesso à conta
        /// </summary>
        public bool TemAcesso(int usuarioId)
        {
            return ContaUsuarios?.Any(cu => cu.UsuarioId == usuarioId && cu.Ativo) ?? false;
        }
    }
}

