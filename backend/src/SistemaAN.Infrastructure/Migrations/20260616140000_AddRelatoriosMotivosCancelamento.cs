using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatoriosMotivosCancelamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "motivos_cancelamento",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motivos_cancelamento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_motivos_cancelamento_nome",
                schema: "public",
                table: "motivos_cancelamento",
                column: "nome",
                unique: true);

            migrationBuilder.AddColumn<long>(
                name: "motivo_cancelamento_id",
                schema: "public",
                table: "clientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacao_cancelamento",
                schema: "public",
                table: "clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "usuario_cancelamento_id",
                schema: "public",
                table: "clientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_motivo_cancelamento",
                schema: "public",
                table: "clientes",
                column: "motivo_cancelamento_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clientes_motivo_cancelamento",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "motivo_cancelamento_id",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "observacao_cancelamento",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "usuario_cancelamento_id",
                schema: "public",
                table: "clientes");

            migrationBuilder.DropTable(
                name: "motivos_cancelamento",
                schema: "public");
        }
    }
}
