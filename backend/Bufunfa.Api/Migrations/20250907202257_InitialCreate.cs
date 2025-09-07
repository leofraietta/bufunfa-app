using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bufunfa.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiaFechamento = table.Column<int>(type: "integer", nullable: true),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TaxaJurosRotativo = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TaxaMultaAtraso = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    UltimosDigitos = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    NomeImpresso = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Bandeira = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContaCorrenteResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    DiaApuracao = table.Column<int>(type: "integer", nullable: true),
                    ManterSaldoPositivo = table.Column<bool>(type: "boolean", nullable: true),
                    EmApuracao = table.Column<bool>(type: "boolean", nullable: true),
                    DataUltimaApuracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorUltimaApuracao = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    LimiteChequeEspecial = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TaxaJurosChequeEspecial = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    NumeroConta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumeroAgencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NomeBanco = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TipoInvestimento = table.Column<int>(type: "integer", nullable: true),
                    NomeProduto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InstituicaoFinanceira = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TaxaRentabilidadeEsperada = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorInvestidoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ValorAtualInvestimento = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DataUltimaAtualizacaoValor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PermiteResgateParcial = table.Column<bool>(type: "boolean", nullable: true),
                    ValorMinimoResgate = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PrazoCarenciaDias = table.Column<int>(type: "integer", nullable: true),
                    CodigoAtivo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxaRendimentoMensal = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    DiaAniversario = table.Column<int>(type: "integer", nullable: true),
                    ValorMinimoManutencao = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ContaPoupanca_NumeroConta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContaPoupanca_NumeroAgencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContaPoupanca_NomeBanco = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Contas_ContaCorrenteResponsavelId",
                        column: x => x.ContaCorrenteResponsavelId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GoogleId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaOrigemId = table.Column<int>(type: "integer", nullable: false),
                    ContaDestinoId = table.Column<int>(type: "integer", nullable: false),
                    TipoRelacionamento = table.Column<int>(type: "integer", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Contas_ContaDestinoId",
                        column: x => x.ContaDestinoId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Contas_ContaOrigemId",
                        column: x => x.ContaOrigemId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ValorProvisionadoMensal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContaUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    EhProprietario = table.Column<bool>(type: "boolean", nullable: false),
                    PercentualParticipacao = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EhAdministrador = table.Column<bool>(type: "boolean", nullable: false),
                    NivelPermissao = table.Column<int>(type: "integer", nullable: false),
                    ConvidadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataConvite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataVinculacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataDesvinculacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContaUsuarios_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContaUsuarios_Usuarios_ConvidadoPorUsuarioId",
                        column: x => x.ConvidadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContaUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlySheetStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataReabertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlySheetStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlySheetStatuses_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonthlySheetStatuses_Usuarios_UsuarioResponsavelId",
                        column: x => x.UsuarioResponsavelId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rateios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PercentualRateio = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ContaConjuntaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rateios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rateios_Contas_ContaConjuntaId",
                        column: x => x.ContaConjuntaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rateios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ValorProvisionado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorReal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DataInicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    TipoRecorrencia = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataFinal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantidadeParcelas = table.Column<int>(type: "integer", nullable: true),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: true),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: true),
                    TipoPeriodicidade = table.Column<int>(type: "integer", nullable: true),
                    IntervaloDias = table.Column<int>(type: "integer", nullable: true),
                    AjustarDiaUtil = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessarRetroativo = table.Column<bool>(type: "boolean", nullable: false),
                    UltimaDataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PermiteAlteracaoValorParcela = table.Column<bool>(type: "boolean", nullable: true),
                    DiaDaSemana = table.Column<int>(type: "integer", nullable: true),
                    DiaDoAno = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lancamentos_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lancamentos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionamentosMercado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    ValorProvisionado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorGastoReal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionamentosMercado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvisionamentosMercado_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionamentosMercado_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvisionamentosMercado_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolhasMensais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    SaldoInicialReal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoInicialProvisionado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoFinalReal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoFinalProvisionado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalReceitasReais = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalReceitasProvisionadas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDespesasReais = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDespesasProvisionadas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Fechada = table.Column<bool>(type: "boolean", nullable: false),
                    MonthlySheetStatusId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolhasMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolhasMensais_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolhasMensais_MonthlySheetStatuses_MonthlySheetStatusId",
                        column: x => x.MonthlySheetStatusId,
                        principalTable: "MonthlySheetStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FolhasMensais_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GastosReaisMercado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProvisionamentoMercadoId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosReaisMercado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastosReaisMercado_ProvisionamentosMercado_ProvisionamentoM~",
                        column: x => x.ProvisionamentoMercadoId,
                        principalTable: "ProvisionamentosMercado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LancamentosFolha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolhaMensalId = table.Column<int>(type: "integer", nullable: false),
                    LancamentoOrigemId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ValorProvisionado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorReal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DataPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRealizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    TipoRecorrencia = table.Column<int>(type: "integer", nullable: false),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: true),
                    TotalParcelas = table.Column<int>(type: "integer", nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true),
                    Realizado = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosFolha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosFolha_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LancamentosFolha_FolhasMensais_FolhaMensalId",
                        column: x => x.FolhaMensalId,
                        principalTable: "FolhasMensais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LancamentosFolha_Lancamentos_LancamentoOrigemId",
                        column: x => x.LancamentoOrigemId,
                        principalTable: "Lancamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationship_Origem_Destino_Tipo",
                table: "AccountRelationships",
                columns: new[] { "ContaOrigemId", "ContaDestinoId", "TipoRelacionamento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_ContaDestinoId",
                table: "AccountRelationships",
                column: "ContaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_CriadoPorUsuarioId",
                table: "AccountRelationships",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioId",
                table: "Categorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_ContaCorrenteResponsavelId",
                table: "Contas",
                column: "ContaCorrenteResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaUsuario_Conta_Usuario",
                table: "ContaUsuarios",
                columns: new[] { "ContaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContaUsuarios_ConvidadoPorUsuarioId",
                table: "ContaUsuarios",
                column: "ConvidadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaUsuarios_UsuarioId",
                table: "ContaUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FolhaMensal_Usuario_Conta_Ano_Mes",
                table: "FolhasMensais",
                columns: new[] { "UsuarioId", "ContaId", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolhasMensais_ContaId",
                table: "FolhasMensais",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_FolhasMensais_MonthlySheetStatusId",
                table: "FolhasMensais",
                column: "MonthlySheetStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosReaisMercado_ProvisionamentoMercadoId",
                table: "GastosReaisMercado",
                column: "ProvisionamentoMercadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CategoriaId",
                table: "Lancamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContaId",
                table: "Lancamentos",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_UsuarioId",
                table: "Lancamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFolha_CategoriaId",
                table: "LancamentosFolha",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFolha_FolhaMensalId",
                table: "LancamentosFolha",
                column: "FolhaMensalId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFolha_LancamentoOrigemId",
                table: "LancamentosFolha",
                column: "LancamentoOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySheetStatus_Conta_Ano_Mes",
                table: "MonthlySheetStatuses",
                columns: new[] { "ContaId", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySheetStatuses_UsuarioResponsavelId",
                table: "MonthlySheetStatuses",
                column: "UsuarioResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionamentoMercado_Usuario_Conta_Categoria_Ano_Mes",
                table: "ProvisionamentosMercado",
                columns: new[] { "UsuarioId", "ContaId", "CategoriaId", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionamentosMercado_CategoriaId",
                table: "ProvisionamentosMercado",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionamentosMercado_ContaId",
                table: "ProvisionamentosMercado",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rateios_ContaConjuntaId",
                table: "Rateios",
                column: "ContaConjuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rateios_UsuarioId",
                table: "Rateios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRelationships");

            migrationBuilder.DropTable(
                name: "ContaUsuarios");

            migrationBuilder.DropTable(
                name: "GastosReaisMercado");

            migrationBuilder.DropTable(
                name: "LancamentosFolha");

            migrationBuilder.DropTable(
                name: "Rateios");

            migrationBuilder.DropTable(
                name: "ProvisionamentosMercado");

            migrationBuilder.DropTable(
                name: "FolhasMensais");

            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "MonthlySheetStatuses");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
