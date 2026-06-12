using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pets",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    raca = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    peso_kg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    idade_aprox = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    sexo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacoes_gerais = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observacoes_alimentares = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    gramas_dia_ajustadas = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pets", x => x.id);
                    table.CheckConstraint("ck_pets_gramas_ajustadas", "gramas_dia_ajustadas IS NULL OR gramas_dia_ajustadas >= 0");
                    table.CheckConstraint("ck_pets_peso", "peso_kg > 0");
                    table.ForeignKey(
                        name: "fk_pets_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "public",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pets_cliente_id",
                schema: "public",
                table: "pets",
                column: "cliente_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pets",
                schema: "public");
        }
    }
}
