using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreferenciasAlimentarias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preferencias_AlimentosExcluidos",
                table: "UsuariosPerfil",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Preferencias_DietasSeguidas",
                table: "UsuariosPerfil",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preferencias_AlimentosExcluidos",
                table: "UsuariosPerfil");

            migrationBuilder.DropColumn(
                name: "Preferencias_DietasSeguidas",
                table: "UsuariosPerfil");
        }
    }
}
