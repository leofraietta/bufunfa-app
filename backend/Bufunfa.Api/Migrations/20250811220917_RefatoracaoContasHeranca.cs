using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bufunfa.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoContasHeranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas");

            migrationBuilder.DropForeignKey(
                name: "FK_Rateios_ContasConjuntas_ContaConjuntaId",
                table: "Rateios");

            migrationBuilder.DropTable(
                name: "ContasConjuntas");

            migrationBuilder.DropIndex(
                name: "IX_Contas_UsuarioId",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Contas");

            migrationBuilder.RenameColumn(
                name: "DataFechamento",
                table: "Contas",
                newName: "DataUltimaAtualizacaoValor");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaAtualizacao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativa",
                table: "Contas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Bandeira",
                table: "Contas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoAtivo",
                table: "Contas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContaCorrenteResponsavelId",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContaPoupanca_NomeBanco",
                table: "Contas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContaPoupanca_NumeroAgencia",
                table: "Contas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContaPoupanca_NumeroConta",
                table: "Contas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Contas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Contas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaApuracao",
                table: "Contas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Contas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DiaAniversario",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaApuracao",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaFechamento",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaVencimento",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmApuracao",
                table: "Contas",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstituicaoFinanceira",
                table: "Contas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteChequeEspecial",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCredito",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManterSaldoPositivo",
                table: "Contas",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeBanco",
                table: "Contas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeImpresso",
                table: "Contas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeProduto",
                table: "Contas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroAgencia",
                table: "Contas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroConta",
                table: "Contas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteResgateParcial",
                table: "Contas",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrazoCarenciaDias",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoAtual",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaJurosChequeEspecial",
                table: "Contas",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaJurosRotativo",
                table: "Contas",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaMultaAtraso",
                table: "Contas",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaRendimentoMensal",
                table: "Contas",
                type: "numeric(5,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaRentabilidadeEsperada",
                table: "Contas",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoInvestimento",
                table: "Contas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltimosDigitos",
                table: "Contas",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAtualInvestimento",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorInvestidoInicial",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorMinimoManutencao",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorMinimoResgate",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorUltimaApuracao",
                table: "Contas",
                type: "numeric(18,2)",
                nullable: true);

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
                    PodeLer = table.Column<bool>(type: "boolean", nullable: false),
                    PodeEscrever = table.Column<bool>(type: "boolean", nullable: false),
                    PodeAdministrar = table.Column<bool>(type: "boolean", nullable: false),
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
                        name: "FK_ContaUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_ContaUsuarios_UsuarioId",
                table: "ContaUsuarios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_Contas_ContaCorrenteResponsavelId",
                table: "Contas",
                column: "ContaCorrenteResponsavelId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rateios_Contas_ContaConjuntaId",
                table: "Rateios",
                column: "ContaConjuntaId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_Contas_ContaCorrenteResponsavelId",
                table: "Contas");

            migrationBuilder.DropForeignKey(
                name: "FK_Rateios_Contas_ContaConjuntaId",
                table: "Rateios");

            migrationBuilder.DropTable(
                name: "ContaUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_Contas_ContaCorrenteResponsavelId",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataUltimaAtualizacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Ativa",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Bandeira",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "CodigoAtivo",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ContaCorrenteResponsavelId",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ContaPoupanca_NomeBanco",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ContaPoupanca_NumeroAgencia",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ContaPoupanca_NumeroConta",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DataUltimaApuracao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DiaAniversario",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DiaApuracao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DiaFechamento",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DiaVencimento",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "EmApuracao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "InstituicaoFinanceira",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "LimiteChequeEspecial",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "LimiteCredito",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ManterSaldoPositivo",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NomeBanco",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NomeImpresso",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NomeProduto",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NumeroAgencia",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NumeroConta",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "PermiteResgateParcial",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "PrazoCarenciaDias",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "SaldoAtual",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TaxaJurosChequeEspecial",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TaxaJurosRotativo",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TaxaMultaAtraso",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TaxaRendimentoMensal",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TaxaRentabilidadeEsperada",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "TipoInvestimento",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "UltimosDigitos",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ValorAtualInvestimento",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ValorInvestidoInicial",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ValorMinimoManutencao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ValorMinimoResgate",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "ValorUltimaApuracao",
                table: "Contas");

            migrationBuilder.RenameColumn(
                name: "DataUltimaAtualizacaoValor",
                table: "Contas",
                newName: "DataFechamento");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Contas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContasConjuntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioCriadorId = table.Column<int>(type: "integer", nullable: false),
                    DataApuracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManterSaldoPositivo = table.Column<bool>(type: "boolean", nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasConjuntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasConjuntas_Usuarios_UsuarioCriadorId",
                        column: x => x.UsuarioCriadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contas_UsuarioId",
                table: "Contas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasConjuntas_UsuarioCriadorId",
                table: "ContasConjuntas",
                column: "UsuarioCriadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rateios_ContasConjuntas_ContaConjuntaId",
                table: "Rateios",
                column: "ContaConjuntaId",
                principalTable: "ContasConjuntas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
