using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SustitucionesDeEjercicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferenciasEntrenamiento_Sustituciones",
                table: "UsuariosPerfil",
                type: "nvarchar(max)",
                nullable: false,
                // "{}" y no "" (lo que genera EF, ver D-37): la columna se lee con
                // JsonSerializer, que revienta con la cadena vacia. Es un mapa, no una lista.
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferenciasEntrenamiento_Sustituciones",
                table: "UsuariosPerfil");
        }
    }
}
