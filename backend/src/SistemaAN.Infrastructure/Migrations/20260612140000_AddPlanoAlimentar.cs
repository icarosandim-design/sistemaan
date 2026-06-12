using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanoAlimentar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "planos_alimentares",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pet_id = table.Column<long>(type: "bigint", nullable: false),
                    frequencia_entrega_id = table.Column<long>(type: "bigint", nullable: false),
                    primeira_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                    gramas_dia_sugeridas = table.Column<int>(type: "integer", nullable: true),
                    gramas_dia_ajustadas = table.Column<int>(type: "integer", nullable: true),
                    tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_planos_alimentares", x => x.id);
                    table.CheckConstraint("ck_planos_alimentares_gramas", "(gramas_dia_sugeridas IS NULL OR gramas_dia_sugeridas >= 0) AND (gramas_dia_ajustadas IS NULL OR gramas_dia_ajustadas >= 0)");
                    table.ForeignKey(
                        name: "fk_planos_alimentares_pet",
                        column: x => x.pet_id,
                        principalSchema: "public",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_planos_alimentares_frequencia",
                        column: x => x.frequencia_entrega_id,
                        principalSchema: "public",
                        principalTable: "frequencias_entrega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plano_itens_receita",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plano_alimentar_id = table.Column<long>(type: "bigint", nullable: false),
                    receita_id = table.Column<long>(type: "bigint", nullable: false),
                    quantidade_ciclo_gramas = table.Column<int>(type: "integer", nullable: true),
                    quantidade_pacotes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_itens_receita", x => x.id);
                    table.CheckConstraint("ck_plano_itens_receita_ciclo", "quantidade_ciclo_gramas IS NULL OR quantidade_ciclo_gramas > 0");
                    table.CheckConstraint("ck_plano_itens_receita_pacotes", "quantidade_pacotes IS NULL OR quantidade_pacotes > 0");
                    table.ForeignKey(
                        name: "fk_plano_itens_receita_plano",
                        column: x => x.plano_alimentar_id,
                        principalSchema: "public",
                        principalTable: "planos_alimentares",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plano_itens_receita_receita",
                        column: x => x.receita_id,
                        principalSchema: "public",
                        principalTable: "receitas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plano_item_pacotes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plano_item_receita_id = table.Column<long>(type: "bigint", nullable: false),
                    tamanho_pacote_id = table.Column<long>(type: "bigint", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_item_pacotes", x => x.id);
                    table.CheckConstraint("ck_plano_item_pacotes_qtd", "quantidade > 0");
                    table.ForeignKey(
                        name: "fk_plano_item_pacotes_item",
                        column: x => x.plano_item_receita_id,
                        principalSchema: "public",
                        principalTable: "plano_itens_receita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plano_item_pacotes_tamanho",
                        column: x => x.tamanho_pacote_id,
                        principalSchema: "public",
                        principalTable: "tamanhos_pacote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_planos_alimentares_frequencia",
                schema: "public",
                table: "planos_alimentares",
                column: "frequencia_entrega_id");

            migrationBuilder.CreateIndex(
                name: "ix_planos_alimentares_pet",
                schema: "public",
                table: "planos_alimentares",
                column: "pet_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plano_itens_receita_plano",
                schema: "public",
                table: "plano_itens_receita",
                column: "plano_alimentar_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_itens_receita_receita",
                schema: "public",
                table: "plano_itens_receita",
                column: "receita_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_item_pacotes_item",
                schema: "public",
                table: "plano_item_pacotes",
                column: "plano_item_receita_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_item_pacotes_tamanho",
                schema: "public",
                table: "plano_item_pacotes",
                column: "tamanho_pacote_id");

            migrationBuilder.CreateIndex(
                name: "ix_receitas_pet",
                schema: "public",
                table: "receitas",
                column: "pet_id");

            migrationBuilder.AddForeignKey(
                name: "fk_receitas_pet",
                schema: "public",
                table: "receitas",
                column: "pet_id",
                principalSchema: "public",
                principalTable: "pets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_receitas_pet",
                schema: "public",
                table: "receitas");

            migrationBuilder.DropIndex(
                name: "ix_receitas_pet",
                schema: "public",
                table: "receitas");

            migrationBuilder.DropTable(
                name: "plano_item_pacotes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "plano_itens_receita",
                schema: "public");

            migrationBuilder.DropTable(
                name: "planos_alimentares",
                schema: "public");
        }
    }
}
