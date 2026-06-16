using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutosVendaAvulsaNotaCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------- produtos ----------------
            migrationBuilder.CreateTable(
                name: "produtos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receita_casa_id = table.Column<long>(type: "bigint", nullable: true),
                    tamanho_pacote_id = table.Column<long>(type: "bigint", nullable: true),
                    unidade_medida = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    peso_gramas = table.Column<int>(type: "integer", nullable: true),
                    preco_venda_avulsa_pf = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    preco_venda_pj = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    controla_estoque = table.Column<bool>(type: "boolean", nullable: false),
                    produzido_internamente = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.ForeignKey("fk_produtos_receita", x => x.receita_casa_id, "receitas", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("fk_produtos_tamanho", x => x.tamanho_pacote_id, "tamanhos_pacote", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                });

            // ---------------- notas_compra ----------------
            migrationBuilder.CreateTable(
                name: "notas_compra",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fornecedor_id = table.Column<long>(type: "bigint", nullable: true),
                    data_compra = table.Column<DateOnly>(type: "date", nullable: false),
                    data_entrada = table.Column<DateOnly>(type: "date", nullable: false),
                    numero_nota_fiscal = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    serie_nota_fiscal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    chave_acesso_nota_fiscal = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_emissao_nota_fiscal = table.Column<DateOnly>(type: "date", nullable: true),
                    data_vencimento_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    forma_pagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    condicao_pagamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    linha_digitavel_boleto = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    codigo_barras_boleto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    banco_emissor_boleto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero_documento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    valor_produtos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    acrescimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notas_compra", x => x.id);
                    table.ForeignKey("fk_notas_compra_fornecedor", x => x.fornecedor_id, "fornecedores", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                });

            // ---------------- vendas_avulsas ----------------
            migrationBuilder.CreateTable(
                name: "vendas_avulsas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    pet_id = table.Column<long>(type: "bigint", nullable: true),
                    entrega_id = table.Column<long>(type: "bigint", nullable: true),
                    data_venda = table.Column<DateOnly>(type: "date", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendas_avulsas", x => x.id);
                    table.ForeignKey("fk_vendas_avulsas_cliente", x => x.cliente_id, "clientes", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("fk_vendas_avulsas_pet", x => x.pet_id, "pets", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                });

            // ---------------- vendas_avulsas_itens ----------------
            migrationBuilder.CreateTable(
                name: "vendas_avulsas_itens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venda_avulsa_id = table.Column<long>(type: "bigint", nullable: false),
                    produto_id = table.Column<long>(type: "bigint", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendas_avulsas_itens", x => x.id);
                    table.ForeignKey("fk_vendas_avulsas_itens_venda", x => x.venda_avulsa_id, "vendas_avulsas", "id", principalSchema: "public", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("fk_vendas_avulsas_itens_produto", x => x.produto_id, "produtos", "id", principalSchema: "public", onDelete: ReferentialAction.Restrict);
                });

            // ---------------- novas colunas (FK opcionais) ----------------
            migrationBuilder.AddColumn<long>(name: "produto_id", schema: "public", table: "itens_estoque", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "nota_compra_id", schema: "public", table: "entradas_estoque", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "produto_id", schema: "public", table: "pedido_itens", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "produto_id", schema: "public", table: "entrega_itens", type: "bigint", nullable: true);

            // ---------------- índices ----------------
            migrationBuilder.CreateIndex("ix_produtos_receita_tamanho", "produtos", new[] { "receita_casa_id", "tamanho_pacote_id" }, schema: "public", unique: true, filter: "receita_casa_id IS NOT NULL AND tamanho_pacote_id IS NOT NULL");
            migrationBuilder.CreateIndex("ix_produtos_codigo", "produtos", "codigo", schema: "public", unique: true, filter: "codigo IS NOT NULL");
            migrationBuilder.CreateIndex("ix_produtos_nome", "produtos", "nome", schema: "public");
            migrationBuilder.CreateIndex("ix_produtos_tipo", "produtos", "tipo", schema: "public");
            migrationBuilder.CreateIndex("ix_produtos_receita", "produtos", "receita_casa_id", schema: "public");
            migrationBuilder.CreateIndex("ix_produtos_tamanho_pacote_id", "produtos", "tamanho_pacote_id", schema: "public");

            migrationBuilder.CreateIndex("ix_notas_compra_fornecedor", "notas_compra", "fornecedor_id", schema: "public");
            migrationBuilder.CreateIndex("ix_notas_compra_data", "notas_compra", "data_compra", schema: "public");
            migrationBuilder.CreateIndex("ix_notas_compra_vencimento", "notas_compra", "data_vencimento_pagamento", schema: "public");

            migrationBuilder.CreateIndex("ix_vendas_avulsas_cliente", "vendas_avulsas", "cliente_id", schema: "public");
            migrationBuilder.CreateIndex("ix_vendas_avulsas_pet", "vendas_avulsas", "pet_id", schema: "public");
            migrationBuilder.CreateIndex("ix_vendas_avulsas_data", "vendas_avulsas", "data_venda", schema: "public");
            migrationBuilder.CreateIndex("ix_vendas_avulsas_entrega", "vendas_avulsas", "entrega_id", schema: "public");
            migrationBuilder.CreateIndex("ix_vendas_avulsas_status", "vendas_avulsas", "status", schema: "public");

            migrationBuilder.CreateIndex("ix_vendas_avulsas_itens_venda", "vendas_avulsas_itens", "venda_avulsa_id", schema: "public");
            migrationBuilder.CreateIndex("ix_vendas_avulsas_itens_produto", "vendas_avulsas_itens", "produto_id", schema: "public");

            migrationBuilder.CreateIndex("ix_itens_estoque_produto", "itens_estoque", "produto_id", schema: "public");
            migrationBuilder.CreateIndex("ix_entradas_estoque_nota_compra", "entradas_estoque", "nota_compra_id", schema: "public");
            migrationBuilder.CreateIndex("ix_pedido_itens_produto", "pedido_itens", "produto_id", schema: "public");
            migrationBuilder.CreateIndex("ix_entrega_itens_produto", "entrega_itens", "produto_id", schema: "public");

            // ---------------- FKs das novas colunas ----------------
            migrationBuilder.AddForeignKey("fk_itens_estoque_produto", "itens_estoque", "produto_id", "produtos", schema: "public", principalSchema: "public", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey("fk_entradas_estoque_nota_compra", "entradas_estoque", "nota_compra_id", "notas_compra", schema: "public", principalSchema: "public", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey("fk_pedido_itens_produto", "pedido_itens", "produto_id", "produtos", schema: "public", principalSchema: "public", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey("fk_entrega_itens_produto", "entrega_itens", "produto_id", "produtos", schema: "public", principalSchema: "public", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("fk_itens_estoque_produto", "itens_estoque", schema: "public");
            migrationBuilder.DropForeignKey("fk_entradas_estoque_nota_compra", "entradas_estoque", schema: "public");
            migrationBuilder.DropForeignKey("fk_pedido_itens_produto", "pedido_itens", schema: "public");
            migrationBuilder.DropForeignKey("fk_entrega_itens_produto", "entrega_itens", schema: "public");

            migrationBuilder.DropIndex("ix_itens_estoque_produto", "itens_estoque", schema: "public");
            migrationBuilder.DropIndex("ix_entradas_estoque_nota_compra", "entradas_estoque", schema: "public");
            migrationBuilder.DropIndex("ix_pedido_itens_produto", "pedido_itens", schema: "public");
            migrationBuilder.DropIndex("ix_entrega_itens_produto", "entrega_itens", schema: "public");

            migrationBuilder.DropColumn("produto_id", "itens_estoque", schema: "public");
            migrationBuilder.DropColumn("nota_compra_id", "entradas_estoque", schema: "public");
            migrationBuilder.DropColumn("produto_id", "pedido_itens", schema: "public");
            migrationBuilder.DropColumn("produto_id", "entrega_itens", schema: "public");

            migrationBuilder.DropTable("vendas_avulsas_itens", schema: "public");
            migrationBuilder.DropTable("vendas_avulsas", schema: "public");
            migrationBuilder.DropTable("notas_compra", schema: "public");
            migrationBuilder.DropTable("produtos", schema: "public");
        }
    }
}
