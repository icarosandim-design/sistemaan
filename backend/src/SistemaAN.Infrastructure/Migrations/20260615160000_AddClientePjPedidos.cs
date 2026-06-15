using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePjPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----- Cliente: natureza PF/PJ (default PF para os existentes) -----
            migrationBuilder.AddColumn<string>(
                name: "natureza",
                schema: "public",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PessoaFisica");

            migrationBuilder.CreateIndex(name: "ix_clientes_natureza", schema: "public", table: "clientes", column: "natureza");

            // ----- Entregas: vínculo com Pedido PJ -----
            migrationBuilder.AddColumn<long>(
                name: "pedido_id", schema: "public", table: "entregas", type: "bigint", nullable: true);
            migrationBuilder.CreateIndex(name: "ix_entregas_pedido", schema: "public", table: "entregas", column: "pedido_id");

            // ----- entrega_pets: PetId nullable + remove FK rígida (id-only) -----
            migrationBuilder.DropForeignKey(name: "fk_entrega_pets_pet", schema: "public", table: "entrega_pets");
            migrationBuilder.AlterColumn<long>(
                name: "pet_id",
                schema: "public",
                table: "entrega_pets",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            // ----- cliente_pj (1:1 com clientes) -----
            migrationBuilder.CreateTable(
                name: "cliente_pj",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    razao_social = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    inscricao_estadual = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    whatsapp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    pessoa_contato = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cargo_contato = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    entrega_rua = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    entrega_numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    entrega_complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    entrega_bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    entrega_cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    entrega_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    entrega_cep = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    tipo_pj = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    condicao_comercial = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    prazo_pagamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    dia_entrega_preferencial = table.Column<int>(type: "integer", nullable: true),
                    frequencia_compra = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    observacoes_comerciais = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_pj", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_pj_cliente",
                        column: x => x.cliente_id,
                        principalSchema: "public",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "ix_cliente_pj_cliente", schema: "public", table: "cliente_pj", column: "cliente_id", unique: true);
            migrationBuilder.CreateIndex(name: "ix_cliente_pj_cnpj", schema: "public", table: "cliente_pj", column: "cnpj");

            // ----- pedidos -----
            migrationBuilder.CreateTable(
                name: "pedidos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_nome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    data_pedido = table.Column<DateOnly>(type: "date", nullable: false),
                    data_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    entrega_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedidos", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedidos_cliente",
                        column: x => x.cliente_id,
                        principalSchema: "public",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "ix_pedidos_cliente", schema: "public", table: "pedidos", column: "cliente_id");
            migrationBuilder.CreateIndex(name: "ix_pedidos_data_entrega", schema: "public", table: "pedidos", column: "data_entrega");
            migrationBuilder.CreateIndex(name: "ix_pedidos_status", schema: "public", table: "pedidos", column: "status");

            // ----- pedido_itens -----
            migrationBuilder.CreateTable(
                name: "pedido_itens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<long>(type: "bigint", nullable: false),
                    receita_id = table.Column<long>(type: "bigint", nullable: false),
                    receita_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    receita_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tamanho_pacote_id = table.Column<long>(type: "bigint", nullable: false),
                    tamanho_nome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    peso_gramas = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedido_itens_pedido",
                        column: x => x.pedido_id,
                        principalSchema: "public",
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "ix_pedido_itens_pedido", schema: "public", table: "pedido_itens", column: "pedido_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "pedido_itens", schema: "public");
            migrationBuilder.DropTable(name: "pedidos", schema: "public");
            migrationBuilder.DropTable(name: "cliente_pj", schema: "public");

            migrationBuilder.DropIndex(name: "ix_entregas_pedido", schema: "public", table: "entregas");
            migrationBuilder.DropColumn(name: "pedido_id", schema: "public", table: "entregas");

            migrationBuilder.DropIndex(name: "ix_clientes_natureza", schema: "public", table: "clientes");
            migrationBuilder.DropColumn(name: "natureza", schema: "public", table: "clientes");

            migrationBuilder.AlterColumn<long>(
                name: "pet_id",
                schema: "public",
                table: "entrega_pets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_entrega_pets_pet",
                schema: "public",
                table: "entrega_pets",
                column: "pet_id",
                principalSchema: "public",
                principalTable: "pets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
