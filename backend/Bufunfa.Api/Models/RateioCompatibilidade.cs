using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    /// <summary>
    /// Classe para compatibilidade com código existente que espera a estrutura antiga de Rateio
    /// Esta classe simula a interface do antigo modelo Rateio usando os dados de ContaUsuario
    /// </summary>
    [NotMapped]
    public class RateioCompatibilidade
    {
        public int Id { get; set; }
        
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentualRateio { get; set; }

        // Chaves estrangeiras
        public int ContaConjuntaId { get; set; }
        public ContaConjunta ContaConjunta { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
