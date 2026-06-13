using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProducao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ordens_producao",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    finalizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    finalizada_por = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    tudo_produzido = table.Column<bool>(type: "boolean", nullable: true),
                    observacoes_finalizacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ordens_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fichas_producao",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_producao_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    entrega_id = table.Column<long>(type: "bigint", nullable: true),
                    entrega_pet_id = table.Column<long>(type: "bigint", nullable: true),
                    entrega_item_id = table.Column<long>(type: "bigint", nullable: true),
                    pet_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    receita_id = table.Column<long>(type: "bigint", nullable: true),
                    tamanho_pacote_id = table.Column<long>(type: "bigint", nullable: true),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    pet_nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    receita_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    receita_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    data_entrega = table.Column<DateOnly>(type: "date", nullable: true),
                    quantidade_pacotes = table.Column<int>(type: "integer", nullable: false),
                    peso_pacote_gramas = table.Column<int>(type: "integer", nullable: false),
                    quantidade_total_gramas = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    motivo_nao_feita = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    quantidade_pacotes_real = table.Column<int>(type: "integer", nullable: true),
                    peso_envasado_gramas = table.Column<int>(type: "integer", nullable: true),
                    concluida_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    concluida_por = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fichas_producao", x => x.id);
                    table.ForeignKey(
                        name: "fk_fichas_producao_ordem",
                        column: x => x.ordem_producao_id,
                        principalSchema: "public",
                        principalTable: "ordens_producao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ficha_producao_ingredientes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ficha_producao_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    categoria = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    gramas_cozidas = table.Column<int>(type: "integer", nullable: false),
                    coeficiente = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ficha_producao_ingredientes", x => x.id);
                    table.ForeignKey(
                        name: "fk_ficha_ingr_ficha",
                        column: x => x.ficha_producao_id,
                        principalSchema: "public",
                        principalTable: "fichas_producao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consumo_ingrediente_producao",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_producao_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    item_estoque_id = table.Column<long>(type: "bigint", nullable: true),
                    item_estoque_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    coeficiente = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    unidade_estoque = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    planejado_cozido_gramas = table.Column<int>(type: "integer", nullable: false),
                    planejado_cru_gramas = table.Column<int>(type: "integer", nullable: false),
                    real_cru_gramas = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    real_cozido_gramas = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    baixa_realizada = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consumo_ingrediente_producao", x => x.id);
                    table.ForeignKey(
                        name: "fk_consumo_producao_ordem",
                        column: x => x.ordem_producao_id,
                        principalSchema: "public",
                        principalTable: "ordens_producao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "ix_ordens_producao_data", schema: "public", table: "ordens_producao", column: "data", unique: true);
            migrationBuilder.CreateIndex(name: "ix_fichas_producao_ordem", schema: "public", table: "fichas_producao", column: "ordem_producao_id");
            migrationBuilder.CreateIndex(name: "ix_fichas_producao_entrega_item", schema: "public", table: "fichas_producao", column: "entrega_item_id");
            migrationBuilder.CreateIndex(name: "ix_ficha_ingr_ficha", schema: "public", table: "ficha_producao_ingredientes", column: "ficha_producao_id");
            migrationBuilder.CreateIndex(name: "ix_consumo_producao_ordem", schema: "public", table: "consumo_ingrediente_producao", column: "ordem_producao_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "consumo_ingrediente_producao", schema: "public");
            migrationBuilder.DropTable(name: "ficha_producao_ingredientes", schema: "public");
            migrationBuilder.DropTable(name: "fichas_producao", schema: "public");
            migrationBuilder.DropTable(name: "ordens_producao", schema: "public");
        }
    }
}
