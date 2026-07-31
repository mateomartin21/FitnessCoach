using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreferenciasEntrenamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferenciasEntrenamiento_EquipoDisponible",
                table: "UsuariosPerfil",
                type: "nvarchar(max)",
                nullable: false,
                // "[]" y no "" (lo que genera EF): la columna se lee con JsonSerializer,
                // que revienta con la cadena vacia y tumbaria todos los perfiles ya creados.
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferenciasEntrenamiento_EquipoDisponible",
                table: "UsuariosPerfil");
        }
    }
}
