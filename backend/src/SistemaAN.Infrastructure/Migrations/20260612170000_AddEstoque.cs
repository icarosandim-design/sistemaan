using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fornecedores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    whats_app = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    pessoa_contato = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    categoria = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    prazo_pagamento_dias = table.Column<int>(type: "integer", nullable: true),
                    forma_pagamento_preferida = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    chave_pix = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    dados_bancarios = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itens_estoque",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unidade_medida = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ingrediente_id = table.Column<long>(type: "bigint", nullable: true),
                    receita_id = table.Column<long>(type: "bigint", nullable: true),
                    tamanho_pacote_id = table.Column<long>(type: "bigint", nullable: true),
                    quantidade_atual = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    quantidade_minima = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    custo_medio = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    fornecedor_principal_id = table.Column<long>(type: "bigint", nullable: true),
                    local_armazenamento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    controla_validade = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_itens_estoque_ingrediente",
                        column: x => x.ingrediente_id,
                        principalSchema: "public",
                        principalTable: "ingredientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_estoque_receita",
                        column: x => x.receita_id,
                        principalSchema: "public",
                        principalTable: "receitas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_estoque_tamanho",
                        column: x => x.tamanho_pacote_id,
                        principalSchema: "public",
                        principalTable: "tamanhos_pacote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_itens_estoque_fornecedor",
                        column: x => x.fornecedor_principal_id,
                        principalSchema: "public",
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lotes_estoque",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_entrada = table.Column<DateOnly>(type: "date", nullable: false),
                    validade = table.Column<DateOnly>(type: "date", nullable: true),
                    quantidade_inicial = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    quantidade_atual = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    custo_unitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    fornecedor_id = table.Column<long>(type: "bigint", nullable: true),
                    origem = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    origem_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lotes_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_lotes_estoque_item",
                        column: x => x.item_estoque_id,
                        principalSchema: "public",
                        principalTable: "itens_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lotes_estoque_fornecedor",
                        column: x => x.fornecedor_id,
                        principalSchema: "public",
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entradas_estoque",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    lote_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    fornecedor_id = table.Column<long>(type: "bigint", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unidade_medida = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_compra = table.Column<DateOnly>(type: "date", nullable: false),
                    data_entrada = table.Column<DateOnly>(type: "date", nullable: false),
                    validade = table.Column<DateOnly>(type: "date", nullable: true),
                    lote_codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    local_armazenamento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    usuario = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entradas_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_entradas_estoque_item",
                        column: x => x.item_estoque_id,
                        principalSchema: "public",
                        principalTable: "itens_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entradas_estoque_lote",
                        column: x => x.lote_estoque_id,
                        principalSchema: "public",
                        principalTable: "lotes_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entradas_estoque_fornecedor",
                        column: x => x.fornecedor_id,
                        principalSchema: "public",
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ajustes_estoque",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    lote_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    quantidade_anterior = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    quantidade_nova = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    diferenca = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    usuario = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    data_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ajustes_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_ajustes_estoque_item",
                        column: x => x.item_estoque_id,
                        principalSchema: "public",
                        principalTable: "itens_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ajustes_estoque_lote",
                        column: x => x.lote_estoque_id,
                        principalSchema: "public",
                        principalTable: "lotes_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacoes_estoque",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    lote_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    tipo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    saldo_anterior_item = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    saldo_posterior_item = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    custo_unitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    usuario = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    data_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    entrada_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    ajuste_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    ordem_producao_id = table.Column<long>(type: "bigint", nullable: true),
                    entrega_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimentacoes_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_mov_estoque_item",
                        column: x => x.item_estoque_id,
                        principalSchema: "public",
                        principalTable: "itens_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mov_estoque_lote",
                        column: x => x.lote_estoque_id,
                        principalSchema: "public",
                        principalTable: "lotes_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mov_estoque_entrada",
                        column: x => x.entrada_estoque_id,
                        principalSchema: "public",
                        principalTable: "entradas_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mov_estoque_ajuste",
                        column: x => x.ajuste_estoque_id,
                        principalSchema: "public",
                        principalTable: "ajustes_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "ix_fornecedores_nome", schema: "public", table: "fornecedores", column: "nome");

            migrationBuilder.CreateIndex(name: "ix_itens_estoque_ingrediente", schema: "public", table: "itens_estoque", column: "ingrediente_id", unique: true);
            migrationBuilder.CreateIndex(name: "ix_itens_estoque_receita_tamanho", schema: "public", table: "itens_estoque", columns: ["receita_id", "tamanho_pacote_id"], unique: true);
            migrationBuilder.CreateIndex(name: "ix_itens_estoque_nome", schema: "public", table: "itens_estoque", column: "nome");
            migrationBuilder.CreateIndex(name: "ix_itens_estoque_tipo", schema: "public", table: "itens_estoque", column: "tipo");
            migrationBuilder.CreateIndex(name: "ix_itens_estoque_fornecedor_principal_id", schema: "public", table: "itens_estoque", column: "fornecedor_principal_id");
            migrationBuilder.CreateIndex(name: "ix_itens_estoque_tamanho_pacote_id", schema: "public", table: "itens_estoque", column: "tamanho_pacote_id");

            migrationBuilder.CreateIndex(name: "ix_lotes_estoque_item", schema: "public", table: "lotes_estoque", column: "item_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_lotes_estoque_validade", schema: "public", table: "lotes_estoque", column: "validade");
            migrationBuilder.CreateIndex(name: "ix_lotes_estoque_fornecedor_id", schema: "public", table: "lotes_estoque", column: "fornecedor_id");

            migrationBuilder.CreateIndex(name: "ix_entradas_estoque_item", schema: "public", table: "entradas_estoque", column: "item_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_entradas_estoque_lote_estoque_id", schema: "public", table: "entradas_estoque", column: "lote_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_entradas_estoque_fornecedor_id", schema: "public", table: "entradas_estoque", column: "fornecedor_id");

            migrationBuilder.CreateIndex(name: "ix_ajustes_estoque_item", schema: "public", table: "ajustes_estoque", column: "item_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_ajustes_estoque_lote_estoque_id", schema: "public", table: "ajustes_estoque", column: "lote_estoque_id");

            migrationBuilder.CreateIndex(name: "ix_mov_estoque_item", schema: "public", table: "movimentacoes_estoque", column: "item_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_mov_estoque_data", schema: "public", table: "movimentacoes_estoque", column: "data_hora");
            migrationBuilder.CreateIndex(name: "ix_mov_estoque_tipo", schema: "public", table: "movimentacoes_estoque", column: "tipo");
            migrationBuilder.CreateIndex(name: "ix_mov_estoque_lote_estoque_id", schema: "public", table: "movimentacoes_estoque", column: "lote_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_mov_estoque_entrada_estoque_id", schema: "public", table: "movimentacoes_estoque", column: "entrada_estoque_id");
            migrationBuilder.CreateIndex(name: "ix_mov_estoque_ajuste_estoque_id", schema: "public", table: "movimentacoes_estoque", column: "ajuste_estoque_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "movimentacoes_estoque", schema: "public");
            migrationBuilder.DropTable(name: "ajustes_estoque", schema: "public");
            migrationBuilder.DropTable(name: "entradas_estoque", schema: "public");
            migrationBuilder.DropTable(name: "lotes_estoque", schema: "public");
            migrationBuilder.DropTable(name: "itens_estoque", schema: "public");
            migrationBuilder.DropTable(name: "fornecedores", schema: "public");
        }
    }
}
