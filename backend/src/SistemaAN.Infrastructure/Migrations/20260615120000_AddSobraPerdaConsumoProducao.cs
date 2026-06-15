using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSobraPerdaConsumoProducao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "sobra_gramas",
                schema: "public",
                table: "consumo_ingrediente_producao",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "perda_gramas",
                schema: "public",
                table: "consumo_ingrediente_producao",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "sobra_gramas", schema: "public", table: "consumo_ingrediente_producao");
            migrationBuilder.DropColumn(name: "perda_gramas", schema: "public", table: "consumo_ingrediente_producao");
        }
    }
}
