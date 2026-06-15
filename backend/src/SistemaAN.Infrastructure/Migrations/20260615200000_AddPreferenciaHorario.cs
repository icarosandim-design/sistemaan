using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAN.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferenciaHorario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preferencia_horario",
                schema: "public",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "HorarioComercial");

            migrationBuilder.AddColumn<string>(
                name: "preferencia_horario",
                schema: "public",
                table: "entregas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "HorarioComercial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "preferencia_horario", schema: "public", table: "clientes");
            migrationBuilder.DropColumn(name: "preferencia_horario", schema: "public", table: "entregas");
        }
    }
}
