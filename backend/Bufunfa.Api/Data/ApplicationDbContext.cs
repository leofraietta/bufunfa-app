using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Models;
using Bufunfa.Api.Services;
using Bufunfa.Api.Converters;

namespace Bufunfa.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<ContaCorrente> ContasCorrente { get; set; }
        public DbSet<ContaCartaoCredito> ContasCartaoCredito { get; set; }
        public DbSet<ContaConjunta> ContasConjuntas { get; set; }
        public DbSet<ContaPoupanca> ContasPoupanca { get; set; }
        public DbSet<ContaInvestimento> ContasInvestimento { get; set; }
        public DbSet<ContaUsuario> ContaUsuarios { get; set; }
        public DbSet<Lancamento> Lancamentos { get; set; }
        public DbSet<LancamentoEsporadico> LancamentosEsporadicos { get; set; }
        public DbSet<LancamentoRecorrente> LancamentosRecorrentes { get; set; }
        public DbSet<LancamentoParcelado> LancamentosParcelados { get; set; }
        // LancamentoPeriodico removido - funcionalidade integrada em LancamentoRecorrente
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Rateio> Rateios { get; set; }
        public DbSet<FolhaMensal> FolhasMensais { get; set; }
        public DbSet<LancamentoFolha> LancamentosFolha { get; set; }
        public DbSet<ProvisionamentoMercado> ProvisionamentosMercado { get; set; }
        public DbSet<GastoRealMercado> GastosReaisMercado { get; set; }
        public DbSet<MonthlySheetStatus> MonthlySheetStatuses { get; set; }
        public DbSet<AccountRelationship> AccountRelationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de herança TPH (Table Per Hierarchy) para Conta
            modelBuilder.Entity<Conta>()
                .HasDiscriminator<TipoConta>("Tipo")
                .HasValue<ContaCorrente>(TipoConta.ContaCorrente)
                .HasValue<ContaCartaoCredito>(TipoConta.ContaCartaoCredito)
                .HasValue<ContaConjunta>(TipoConta.ContaConjunta)
                .HasValue<ContaPoupanca>(TipoConta.ContaPoupanca)
                .HasValue<ContaInvestimento>(TipoConta.ContaInvestimento);

            // Configurações para ContaUsuario (relacionamento many-to-many)
            modelBuilder.Entity<ContaUsuario>()
                .HasIndex(cu => new { cu.ContaId, cu.UsuarioId })
                .IsUnique()
                .HasDatabaseName("IX_ContaUsuario_Conta_Usuario");

            modelBuilder.Entity<ContaUsuario>()
                .HasOne(cu => cu.Conta)
                .WithMany(c => c.ContaUsuarios)
                .HasForeignKey(cu => cu.ContaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaUsuario>()
                .HasOne(cu => cu.Usuario)
                .WithMany(u => u.ContaUsuarios)
                .HasForeignKey(cu => cu.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações específicas para ContaCartaoCredito
            modelBuilder.Entity<ContaCartaoCredito>()
                .HasOne(cc => cc.ContaCorrenteResponsavel)
                .WithMany(cr => cr.CartoesCredito)
                .HasForeignKey(cc => cc.ContaCorrenteResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurações para campos decimais
            modelBuilder.Entity<Conta>()
                .Property(c => c.SaldoInicial)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Conta>()
                .Property(c => c.SaldoAtual)
                .HasColumnType("decimal(18,2)");

            // Configurações para ContaUsuario
            modelBuilder.Entity<ContaUsuario>()
                .Property(cu => cu.PercentualParticipacao)
                .HasColumnType("decimal(5,2)");

            // Configuração de herança TPH (Table Per Hierarchy) para Lancamento
            modelBuilder.Entity<Lancamento>()
                .HasDiscriminator<TipoRecorrencia>("TipoRecorrencia")
                .HasValue<LancamentoEsporadico>(TipoRecorrencia.Esporadico)
                .HasValue<LancamentoRecorrente>(TipoRecorrencia.Recorrente)
                .HasValue<LancamentoParcelado>(TipoRecorrencia.Parcelado);

            // Configurações para campos decimais de Lançamento
            modelBuilder.Entity<Lancamento>()
                .Property(l => l.ValorProvisionado)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lancamento>()
                .Property(l => l.ValorReal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LancamentoParcelado>()
                .Property(lp => lp.ValorParcela)
                .HasColumnType("decimal(18,2)");

            // Configurações de timestamps
            modelBuilder.Entity<Conta>()
                .Property(c => c.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ContaUsuario>()
                .Property(cu => cu.DataVinculacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

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

            // Configurações para ProvisionamentoMercado
            modelBuilder.Entity<ProvisionamentoMercado>()
                .HasIndex(p => new { p.UsuarioId, p.ContaId, p.CategoriaId, p.Ano, p.Mes })
                .IsUnique()
                .HasDatabaseName("IX_ProvisionamentoMercado_Usuario_Conta_Categoria_Ano_Mes");

            modelBuilder.Entity<ProvisionamentoMercado>()
                .Property(p => p.ValorProvisionado)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProvisionamentoMercado>()
                .Property(p => p.ValorGastoReal)
                .HasColumnType("decimal(18,2)");

            // Configurações para GastoRealMercado
            modelBuilder.Entity<GastoRealMercado>()
                .HasOne(g => g.ProvisionamentoMercado)
                .WithMany(p => p.GastosReais)
                .HasForeignKey(g => g.ProvisionamentoMercadoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GastoRealMercado>()
                .Property(g => g.Valor)
                .HasColumnType("decimal(18,2)");

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

            // Configurações para ProvisionamentoMercado
            modelBuilder.Entity<ProvisionamentoMercado>()
                .Property(p => p.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para GastoRealMercado
            modelBuilder.Entity<GastoRealMercado>()
                .Property(g => g.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para Lançamento
            modelBuilder.Entity<Lancamento>()
                .Property(l => l.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para MonthlySheetStatus
            modelBuilder.Entity<MonthlySheetStatus>()
                .HasIndex(mss => new { mss.ContaId, mss.Ano, mss.Mes })
                .IsUnique()
                .HasDatabaseName("IX_MonthlySheetStatus_Conta_Ano_Mes");

            modelBuilder.Entity<MonthlySheetStatus>()
                .HasOne(mss => mss.Conta)
                .WithMany(c => c.MonthlySheetStatuses)
                .HasForeignKey(mss => mss.ContaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MonthlySheetStatus>()
                .HasOne(mss => mss.UsuarioResponsavel)
                .WithMany()
                .HasForeignKey(mss => mss.UsuarioResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações para AccountRelationship
            modelBuilder.Entity<AccountRelationship>()
                .HasIndex(ar => new { ar.ContaOrigemId, ar.ContaDestinoId, ar.TipoRelacionamento })
                .IsUnique()
                .HasDatabaseName("IX_AccountRelationship_Origem_Destino_Tipo");

            modelBuilder.Entity<AccountRelationship>()
                .HasOne(ar => ar.ContaOrigem)
                .WithMany(c => c.RelacionamentosOrigem)
                .HasForeignKey(ar => ar.ContaOrigemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AccountRelationship>()
                .HasOne(ar => ar.ContaDestino)
                .WithMany(c => c.RelacionamentosDestino)
                .HasForeignKey(ar => ar.ContaDestinoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações para FolhaMensal com MonthlySheetStatus
            modelBuilder.Entity<FolhaMensal>()
                .HasOne(f => f.MonthlySheetStatus)
                .WithMany()
                .HasForeignKey(f => f.MonthlySheetStatusId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurações para Categoria
            modelBuilder.Entity<Categoria>()
                .Property(c => c.ValorProvisionadoMensal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Categoria>()
                .Property(c => c.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configurações para Lancamento com Categoria
            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.Categoria)
                .WithMany(c => c.Lancamentos)
                .HasForeignKey(l => l.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar conversores UTC para todos os campos DateTime
            ConfigureUtcDateTimeConverters(modelBuilder);
        }

        private void ConfigureUtcDateTimeConverters(ModelBuilder modelBuilder)
        {
            var utcConverter = new UtcDateTimeConverter();
            var utcNullableConverter = new UtcNullableDateTimeConverter();

            // Aplicar conversor UTC para todas as propriedades DateTime
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(utcNullableConverter);
                    }
                }
            }
        }
    }
}

