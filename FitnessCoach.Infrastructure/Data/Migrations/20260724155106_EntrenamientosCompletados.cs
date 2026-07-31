using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntrenamientosCompletados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntrenamientosCompletados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreRutina = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrenamientosCompletados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrenamientosCompletados_UsuariosPerfil_UsuarioPerfilId",
                        column: x => x.UsuarioPerfilId,
                        principalTable: "UsuariosPerfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientosCompletados_UsuarioPerfilId",
                table: "EntrenamientosCompletados",
                column: "UsuarioPerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntrenamientosCompletados");
        }
    }
}
