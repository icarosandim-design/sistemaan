using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entregas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    data_prevista = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    cliente_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    rua = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cep = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    frequencia_nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    dias_ciclo = table.Column<int>(type: "integer", nullable: false),
                    observacoes_internas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observacoes_entregador = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    entregador_id = table.Column<long>(type: "bigint", nullable: true),
                    motivo_nao_entrega = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    motivo_reagendamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    reagendada_de_id = table.Column<long>(type: "bigint", nullable: true),
                    reagendada_para_id = table.Column<long>(type: "bigint", nullable: true),
                    reagendada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cancelada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entregas", x => x.id);
                    table.ForeignKey(
                        name: "fk_entregas_cliente",
                        column: x => x.cliente_id,
                        principalSchema: "public",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrega_pets",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrega_id = table.Column<long>(type: "bigint", nullable: false),
                    pet_id = table.Column<long>(type: "bigint", nullable: false),
                    pet_nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tipo_alimentacao = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    gramas_dia = table.Column<int>(type: "integer", nullable: true),
                    quantidade_total_gramas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_pets", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_pets_entrega",
                        column: x => x.entrega_id,
                        principalSchema: "public",
                        principalTable: "entregas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrega_pets_pet",
                        column: x => x.pet_id,
                        principalSchema: "public",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrega_itens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrega_pet_id = table.Column<long>(type: "bigint", nullable: false),
                    receita_id = table.Column<long>(type: "bigint", nullable: false),
                    receita_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    receita_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    quantidade_ciclo_gramas = table.Column<int>(type: "integer", nullable: true),
                    tamanho_pacote_gramas = table.Column<int>(type: "integer", nullable: true),
                    quantidade_pacotes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_itens_pet",
                        column: x => x.entrega_pet_id,
                        principalSchema: "public",
                        principalTable: "entrega_pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrega_itens_receita",
                        column: x => x.receita_id,
                        principalSchema: "public",
                        principalTable: "receitas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrega_item_pacotes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrega_item_id = table.Column<long>(type: "bigint", nullable: false),
                    tamanho_label = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    peso_gramas = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_item_pacotes", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_pacotes_item",
                        column: x => x.entrega_item_id,
                        principalSchema: "public",
                        principalTable: "entrega_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entrega_item_ingredientes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrega_item_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_id = table.Column<long>(type: "bigint", nullable: false),
                    ingrediente_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    categoria = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    gramas_cozidas = table.Column<int>(type: "integer", nullable: false),
                    custo_kg_cru = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    coeficiente = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_item_ingredientes", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_ingr_item",
                        column: x => x.entrega_item_id,
                        principalSchema: "public",
                        principalTable: "entrega_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrega_ingr_ingrediente",
                        column: x => x.ingrediente_id,
                        principalSchema: "public",
                        principalTable: "ingredientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrega_historico",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrega_id = table.Column<long>(type: "bigint", nullable: false),
                    quando = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    usuario = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    evento = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    status_de = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    status_para = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_historico", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_historico_entrega",
                        column: x => x.entrega_id,
                        principalSchema: "public",
                        principalTable: "entregas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "ix_entregas_cliente_data", schema: "public", table: "entregas", columns: ["cliente_id", "data_prevista"]);
            migrationBuilder.CreateIndex(name: "ix_entregas_data", schema: "public", table: "entregas", column: "data_prevista");
            migrationBuilder.CreateIndex(name: "ix_entregas_status", schema: "public", table: "entregas", column: "status");

            migrationBuilder.CreateIndex(name: "ix_entrega_pets_entrega", schema: "public", table: "entrega_pets", column: "entrega_id");
            migrationBuilder.CreateIndex(name: "ix_entrega_pets_pet", schema: "public", table: "entrega_pets", column: "pet_id");

            migrationBuilder.CreateIndex(name: "ix_entrega_itens_pet", schema: "public", table: "entrega_itens", column: "entrega_pet_id");
            migrationBuilder.CreateIndex(name: "ix_entrega_itens_receita", schema: "public", table: "entrega_itens", column: "receita_id");

            migrationBuilder.CreateIndex(name: "ix_entrega_pacotes_item", schema: "public", table: "entrega_item_pacotes", column: "entrega_item_id");

            migrationBuilder.CreateIndex(name: "ix_entrega_ingr_item", schema: "public", table: "entrega_item_ingredientes", column: "entrega_item_id");
            migrationBuilder.CreateIndex(name: "ix_entrega_ingr_ingrediente", schema: "public", table: "entrega_item_ingredientes", column: "ingrediente_id");

            migrationBuilder.CreateIndex(name: "ix_entrega_historico_entrega", schema: "public", table: "entrega_historico", column: "entrega_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "entrega_historico", schema: "public");
            migrationBuilder.DropTable(name: "entrega_item_ingredientes", schema: "public");
            migrationBuilder.DropTable(name: "entrega_item_pacotes", schema: "public");
            migrationBuilder.DropTable(name: "entrega_itens", schema: "public");
            migrationBuilder.DropTable(name: "entrega_pets", schema: "public");
            migrationBuilder.DropTable(name: "entregas", schema: "public");
        }
    }
}
