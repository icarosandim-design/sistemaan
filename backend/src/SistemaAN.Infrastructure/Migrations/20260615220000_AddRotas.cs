using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rotas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entregador = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rotas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rota_paradas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rota_id = table.Column<long>(type: "bigint", nullable: false),
                    entrega_id = table.Column<long>(type: "bigint", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rota_paradas", x => x.id);
                    table.ForeignKey(
                        name: "fk_rota_paradas_rota",
                        column: x => x.rota_id,
                        principalSchema: "public",
                        principalTable: "rotas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "ix_rotas_data", schema: "public", table: "rotas", column: "data");
            migrationBuilder.CreateIndex(name: "ix_rotas_status", schema: "public", table: "rotas", column: "status");
            migrationBuilder.CreateIndex(name: "ix_rota_paradas_rota", schema: "public", table: "rota_paradas", column: "rota_id");
            migrationBuilder.CreateIndex(name: "ix_rota_paradas_entrega", schema: "public", table: "rota_paradas", column: "entrega_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "rota_paradas", schema: "public");
            migrationBuilder.DropTable(name: "rotas", schema: "public");
        }
    }
}
