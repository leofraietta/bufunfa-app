using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bufunfa.Api.Migrations
{
    /// <inheritdoc />
    public partial class Migration_0001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Lancamentos");

            migrationBuilder.RenameColumn(
                name: "ParcelaAtual",
                table: "Lancamentos",
                newName: "DiaVencimento");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Lancamentos",
                newName: "DataInicial");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorProvisionado",
                table: "Lancamentos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Lancamentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Lancamentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Lancamentos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFinal",
                table: "Lancamentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorReal",
                table: "Lancamentos",
                type: "numeric(18,2)",
                nullable: true);

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
                    Fechada = table.Column<bool>(type: "boolean", nullable: false)
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
                        name: "FK_FolhasMensais_Usuarios_UsuarioId",
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
                name: "IX_GastosReaisMercado_ProvisionamentoMercadoId",
                table: "GastosReaisMercado",
                column: "ProvisionamentoMercadoId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GastosReaisMercado");

            migrationBuilder.DropTable(
                name: "LancamentosFolha");

            migrationBuilder.DropTable(
                name: "ProvisionamentosMercado");

            migrationBuilder.DropTable(
                name: "FolhasMensais");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "DataFinal",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ValorReal",
                table: "Lancamentos");

            migrationBuilder.RenameColumn(
                name: "DiaVencimento",
                table: "Lancamentos",
                newName: "ParcelaAtual");

            migrationBuilder.RenameColumn(
                name: "DataInicial",
                table: "Lancamentos",
                newName: "Data");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorProvisionado",
                table: "Lancamentos",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Lancamentos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
