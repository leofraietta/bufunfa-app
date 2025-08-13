using System.ComponentModel.DataAnnotations;

namespace Bufunfa.Api.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        // Propriedade para armazenar o ID do usuário do Google OAuth2
        [MaxLength(255)]
        public string GoogleId { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataUltimaAtualizacao { get; set; }

        // Relacionamento many-to-many com contas
        public ICollection<ContaUsuario> ContaUsuarios { get; set; } = new List<ContaUsuario>();

        // Relacionamento com categorias
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

        // Relacionamento com folhas mensais
        public ICollection<FolhaMensal> FolhasMensais { get; set; } = new List<FolhaMensal>();

        // Relacionamento com provisionamentos de mercado será adicionado quando o modelo for criado
        // public ICollection<ProvisionamentoMercado> ProvisionamentosMercado { get; set; } = new List<ProvisionamentoMercado>();
    }
}

