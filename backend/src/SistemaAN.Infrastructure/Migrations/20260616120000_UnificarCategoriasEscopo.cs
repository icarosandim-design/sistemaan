using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnificarCategoriasEscopo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "escopo",
                schema: "public",
                table: "categorias_ingredientes",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Alimento");

            migrationBuilder.AlterColumn<string>(
                name: "categoria",
                schema: "public",
                table: "itens_estoque",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "categoria",
                schema: "public",
                table: "itens_estoque",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.DropColumn(name: "escopo", schema: "public", table: "categorias_ingredientes");
        }
    }
}
