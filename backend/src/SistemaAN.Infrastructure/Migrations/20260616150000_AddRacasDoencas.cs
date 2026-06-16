using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRacasDoencas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "racas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_racas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "doencas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_doencas", x => x.id);
                });

            migrationBuilder.AddColumn<long>(
                name: "raca_id",
                schema: "public",
                table: "pets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "pet_doencas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pet_id = table.Column<long>(type: "bigint", nullable: false),
                    doenca_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pet_doencas", x => x.id);
                    table.ForeignKey(
                        name: "fk_pet_doencas_pet",
                        column: x => x.pet_id,
                        principalSchema: "public",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pet_doencas_doenca",
                        column: x => x.doenca_id,
                        principalSchema: "public",
                        principalTable: "doencas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_racas_nome",
                schema: "public",
                table: "racas",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_doencas_nome",
                schema: "public",
                table: "doencas",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pets_raca",
                schema: "public",
                table: "pets",
                column: "raca_id");

            migrationBuilder.CreateIndex(
                name: "ix_pet_doencas_doenca_id",
                schema: "public",
                table: "pet_doencas",
                column: "doenca_id");

            migrationBuilder.CreateIndex(
                name: "ix_pet_doencas_pet_id_doenca_id",
                schema: "public",
                table: "pet_doencas",
                columns: new[] { "pet_id", "doenca_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_pets_raca",
                schema: "public",
                table: "pets",
                column: "raca_id",
                principalSchema: "public",
                principalTable: "racas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pets_raca",
                schema: "public",
                table: "pets");

            migrationBuilder.DropTable(
                name: "pet_doencas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "racas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "doencas",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_pets_raca",
                schema: "public",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "raca_id",
                schema: "public",
                table: "pets");
        }
    }
}
