using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bufunfa.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoLancamentosOOP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiaDaSemana",
                table: "Lancamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaDoAno",
                table: "Lancamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloDias",
                table: "Lancamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParcelaAtual",
                table: "Lancamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteAlteracaoValorParcela",
                table: "Lancamentos",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessarRetroativo",
                table: "Lancamentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoPeriodicidade",
                table: "Lancamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaDataProcessamento",
                table: "Lancamentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorParcela",
                table: "Lancamentos",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaDaSemana",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "DiaDoAno",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "IntervaloDias",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ParcelaAtual",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "PermiteAlteracaoValorParcela",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ProcessarRetroativo",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "TipoPeriodicidade",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "UltimaDataProcessamento",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ValorParcela",
                table: "Lancamentos");
        }
    }
}
