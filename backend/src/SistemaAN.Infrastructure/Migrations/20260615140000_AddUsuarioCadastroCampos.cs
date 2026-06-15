using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioCadastroCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "telefone",
                schema: "public",
                table: "usuarios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacoes",
                schema: "public",
                table: "usuarios",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "telefone", schema: "public", table: "usuarios");
            migrationBuilder.DropColumn(name: "observacoes", schema: "public", table: "usuarios");
        }
    }
}
