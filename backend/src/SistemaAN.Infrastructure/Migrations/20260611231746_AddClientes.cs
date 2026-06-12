using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    endereco = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_cliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dia_cobranca = table.Column<int>(type: "integer", nullable: true),
                    valor_recorrente_mensal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status_financeiro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observacoes_financeiras = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                    table.CheckConstraint("ck_clientes_dia_cobranca", "dia_cobranca IS NULL OR (dia_cobranca BETWEEN 1 AND 31)");
                    table.CheckConstraint("ck_clientes_valor", "valor_recorrente_mensal >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_clientes_nome",
                schema: "public",
                table: "clientes",
                column: "nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes",
                schema: "public");
        }
    }
}
