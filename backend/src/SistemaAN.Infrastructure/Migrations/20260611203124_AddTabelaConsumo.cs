using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelaConsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "faixas_consumo",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    peso_inicial = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    peso_final = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    gramas_por_dia = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faixas_consumo", x => x.id);
                    table.CheckConstraint("ck_faixas_consumo_gramas", "gramas_por_dia > 0");
                    table.CheckConstraint("ck_faixas_consumo_peso", "peso_inicial < peso_final");
                });

            migrationBuilder.CreateIndex(
                name: "ix_faixas_consumo_peso_inicial",
                schema: "public",
                table: "faixas_consumo",
                column: "peso_inicial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "faixas_consumo",
                schema: "public");
        }
    }
}
