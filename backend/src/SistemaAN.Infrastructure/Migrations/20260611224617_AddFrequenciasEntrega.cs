using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFrequenciasEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "frequencias_entrega",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    dias_ciclo = table.Column<int>(type: "integer", nullable: true),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    personalizada = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_frequencias_entrega", x => x.id);
                    table.CheckConstraint("ck_frequencias_entrega_dias", "personalizada = true OR (dias_ciclo IS NOT NULL AND dias_ciclo > 0)");
                });

            migrationBuilder.CreateIndex(
                name: "ix_frequencias_entrega_nome",
                schema: "public",
                table: "frequencias_entrega",
                column: "nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "frequencias_entrega",
                schema: "public");
        }
    }
}
