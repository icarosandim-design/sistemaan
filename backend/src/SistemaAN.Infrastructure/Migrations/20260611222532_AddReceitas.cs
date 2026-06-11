using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "receitas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    pet_id = table.Column<long>(type: "bigint", nullable: true),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receitas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itens_receita",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receita_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_id = table.Column<long>(type: "bigint", nullable: false),
                    gramas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens_receita", x => x.id);
                    table.CheckConstraint("ck_itens_receita_gramas", "gramas > 0");
                    table.ForeignKey(
                        name: "fk_itens_receita_ingredientes_ingrediente_id",
                        column: x => x.ingrediente_id,
                        principalSchema: "public",
                        principalTable: "ingredientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_receita_receitas_receita_id",
                        column: x => x.receita_id,
                        principalSchema: "public",
                        principalTable: "receitas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_itens_receita_ingrediente_id",
                schema: "public",
                table: "itens_receita",
                column: "ingrediente_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_receita_receita_id",
                schema: "public",
                table: "itens_receita",
                column: "receita_id");

            migrationBuilder.CreateIndex(
                name: "ix_receitas_tipo_codigo",
                schema: "public",
                table: "receitas",
                columns: new[] { "tipo", "codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_receita",
                schema: "public");

            migrationBuilder.DropTable(
                name: "receitas",
                schema: "public");
        }
    }
}
