using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Models;

namespace Bufunfa.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Lancamento> Lancamentos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ContaConjunta> ContasConjuntas { get; set; }
        public DbSet<Rateio> Rateios { get; set; }
        public DbSet<FolhaMensal> FolhasMensais { get; set; }
        public DbSet<LancamentoFolha> LancamentosFolha { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações para FolhaMensal
            modelBuilder.Entity<FolhaMensal>()
                .HasIndex(f => new { f.UsuarioId, f.ContaId, f.Ano, f.Mes })
                .IsUnique()
                .HasDatabaseName("IX_FolhaMensal_Usuario_Conta_Ano_Mes");

            // Configurações para LancamentoFolha
            modelBuilder.Entity<LancamentoFolha>()
                .HasOne(lf => lf.FolhaMensal)
                .WithMany(f => f.LancamentosFolha)
                .HasForeignKey(lf => lf.FolhaMensalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LancamentoFolha>()
                .HasOne(lf => lf.LancamentoOrigem)
                .WithMany(l => l.LancamentosFolha)
                .HasForeignKey(lf => lf.LancamentoOrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações para Lancamento
            modelBuilder.Entity<Lancamento>()
                .Property(l => l.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para LancamentoFolha
            modelBuilder.Entity<LancamentoFolha>()
                .Property(lf => lf.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para FolhaMensal
            modelBuilder.Entity<FolhaMensal>()
                .Property(f => f.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}

