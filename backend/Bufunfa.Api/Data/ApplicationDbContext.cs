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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações adicionais do modelo, se necessário
            // Ex: Chaves estrangeiras, índices, etc.
        }
    }
}

