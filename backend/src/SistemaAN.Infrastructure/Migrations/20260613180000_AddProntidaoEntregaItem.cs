using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProntidaoEntregaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status_preparo",
                schema: "public",
                table: "entrega_itens",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "NaoPronta");

            migrationBuilder.AddColumn<int>(
                name: "pacotes_prontos",
                schema: "public",
                table: "entrega_itens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "preparado_em",
                schema: "public",
                table: "entrega_itens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preparado_por",
                schema: "public",
                table: "entrega_itens",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "status_preparo", schema: "public", table: "entrega_itens");
            migrationBuilder.DropColumn(name: "pacotes_prontos", schema: "public", table: "entrega_itens");
            migrationBuilder.DropColumn(name: "preparado_em", schema: "public", table: "entrega_itens");
            migrationBuilder.DropColumn(name: "preparado_por", schema: "public", table: "entrega_itens");
        }
    }
}
