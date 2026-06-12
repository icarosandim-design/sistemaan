using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EntregaNoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove a entrega do plano (passa a ser do cliente).
            migrationBuilder.DropForeignKey(
                name: "fk_planos_alimentares_frequencia",
                schema: "public",
                table: "planos_alimentares");

            migrationBuilder.DropIndex(
                name: "ix_planos_alimentares_frequencia",
                schema: "public",
                table: "planos_alimentares");

            migrationBuilder.DropColumn(
                name: "frequencia_entrega_id",
                schema: "public",
                table: "planos_alimentares");

            migrationBuilder.DropColumn(
                name: "primeira_entrega",
                schema: "public",
                table: "planos_alimentares");

            // Entrega passa para o cliente (compartilhada por todos os pets).
            migrationBuilder.AddColumn<long>(
                name: "frequencia_entrega_id",
                schema: "public",
                table: "clientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "primeira_entrega",
                schema: "public",
                table: "clientes",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_frequencia",
                schema: "public",
                table: "clientes",
                column: "frequencia_entrega_id");

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_frequencia",
                schema: "public",
                table: "clientes",
                column: "frequencia_entrega_id",
                principalSchema: "public",
                principalTable: "frequencias_entrega",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_clientes_frequencia",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "ix_clientes_frequencia",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "frequencia_entrega_id",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "primeira_entrega",
                schema: "public",
                table: "clientes");

            migrationBuilder.AddColumn<long>(
                name: "frequencia_entrega_id",
                schema: "public",
                table: "planos_alimentares",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateOnly>(
                name: "primeira_entrega",
                schema: "public",
                table: "planos_alimentares",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "ix_planos_alimentares_frequencia",
                schema: "public",
                table: "planos_alimentares",
                column: "frequencia_entrega_id");

            migrationBuilder.AddForeignKey(
                name: "fk_planos_alimentares_frequencia",
                schema: "public",
                table: "planos_alimentares",
                column: "frequencia_entrega_id",
                principalSchema: "public",
                principalTable: "frequencias_entrega",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
