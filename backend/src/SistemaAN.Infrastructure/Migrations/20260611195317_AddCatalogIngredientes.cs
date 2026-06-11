using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogIngredientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_ingredientes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias_ingredientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredientes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    categoria_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_conversao = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    coeficiente_conversao = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    custo_atual_kg = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredientes", x => x.id);
                    table.CheckConstraint("ck_ingredientes_coeficiente", "coeficiente_conversao > 0");
                    table.CheckConstraint("ck_ingredientes_custo", "custo_atual_kg >= 0");
                    table.ForeignKey(
                        name: "fk_ingredientes_categorias_ingredientes_categoria_id",
                        column: x => x.categoria_id,
                        principalSchema: "public",
                        principalTable: "categorias_ingredientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categorias_ingredientes_nome",
                schema: "public",
                table: "categorias_ingredientes",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ingredientes_categoria_id",
                schema: "public",
                table: "ingredientes",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_ingredientes_nome",
                schema: "public",
                table: "ingredientes",
                column: "nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingredientes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categorias_ingredientes",
                schema: "public");
        }
    }
}
