using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteEnderecoCpfCancelamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "endereco",
                schema: "public",
                table: "clientes");

            migrationBuilder.AddColumn<string>(
                name: "cep",
                schema: "public",
                table: "clientes",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                schema: "public",
                table: "clientes",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                schema: "public",
                table: "clientes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "data_cancelamento",
                schema: "public",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "public",
                table: "clientes",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_cancelamento",
                schema: "public",
                table: "clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                schema: "public",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origem_venda",
                schema: "public",
                table: "clientes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rua",
                schema: "public",
                table: "clientes",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_cpf",
                schema: "public",
                table: "clientes",
                column: "cpf",
                unique: true,
                filter: "cpf IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clientes_cpf",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cep",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "complemento",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cpf",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "data_cancelamento",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "motivo_cancelamento",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "numero",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "origem_venda",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "rua",
                schema: "public",
                table: "clientes");

            migrationBuilder.AddColumn<string>(
                name: "endereco",
                schema: "public",
                table: "clientes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
