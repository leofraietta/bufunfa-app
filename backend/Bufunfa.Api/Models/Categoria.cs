using System.ComponentModel.DataAnnotations;

namespace Bufunfa.Api.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [MaxLength(500)]
        public string Descricao { get; set; }

        // Chave estrangeira para o usuário
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}

