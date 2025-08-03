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
    }
}

