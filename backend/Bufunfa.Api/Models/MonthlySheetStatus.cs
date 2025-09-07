using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bufunfa.Api.Models
{
    public enum SheetStatus
    {
        Open = 1,
        Closed = 2
    }

    /// <summary>
    /// Gerencia o status de abertura/fechamento das folhas mensais
    /// Implementa as regras de fechamento sequencial definidas nos requisitos
    /// </summary>
    public class MonthlySheetStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaId { get; set; }
        public Conta Conta { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        [Range(1, 12)]
        public int Mes { get; set; }

        [Required]
        public SheetStatus Status { get; set; } = SheetStatus.Open;

        public DateTime? DataFechamento { get; set; }
        public DateTime? DataReabertura { get; set; }

        [Required]
        public int UsuarioResponsavelId { get; set; }
        public Usuario UsuarioResponsavel { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // Propriedades calculadas
        [NotMapped]
        public DateTime DataReferencia => new DateTime(Ano, Mes, 1);

        [NotMapped]
        public string ChaveUnica => $"{ContaId}_{Ano}_{Mes:D2}";

        /// <summary>
        /// Verifica se pode fechar esta folha (M-1 deve estar fechado)
        /// </summary>
        public bool PodeFechar(List<MonthlySheetStatus> todasFolhas)
        {
            if (Status == SheetStatus.Closed) return false;

            var mesAnterior = DataReferencia.AddMonths(-1);
            var folhaAnterior = todasFolhas.FirstOrDefault(f => 
                f.ContaId == ContaId && 
                f.Ano == mesAnterior.Year && 
                f.Mes == mesAnterior.Month);

            // Se não existe folha anterior, pode fechar
            if (folhaAnterior == null) return true;

            // Só pode fechar se a anterior estiver fechada
            return folhaAnterior.Status == SheetStatus.Closed;
        }

        /// <summary>
        /// Verifica se pode reabrir esta folha (M+1 deve estar aberto)
        /// </summary>
        public bool PodeReabrir(List<MonthlySheetStatus> todasFolhas)
        {
            if (Status == SheetStatus.Open) return false;

            var mesPosterior = DataReferencia.AddMonths(1);
            var folhaPosterior = todasFolhas.FirstOrDefault(f => 
                f.ContaId == ContaId && 
                f.Ano == mesPosterior.Year && 
                f.Mes == mesPosterior.Month);

            // Se não existe folha posterior, pode reabrir
            if (folhaPosterior == null) return true;

            // Só pode reabrir se a posterior estiver aberta
            return folhaPosterior.Status == SheetStatus.Open;
        }

        /// <summary>
        /// Fecha a folha mensal
        /// </summary>
        public void Fechar(int usuarioId)
        {
            Status = SheetStatus.Closed;
            DataFechamento = DateTime.UtcNow;
            UsuarioResponsavelId = usuarioId;
            DataAtualizacao = DateTime.UtcNow;
        }

        /// <summary>
        /// Reabre a folha mensal
        /// </summary>
        public void Reabrir(int usuarioId)
        {
            Status = SheetStatus.Open;
            DataReabertura = DateTime.UtcNow;
            UsuarioResponsavelId = usuarioId;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
