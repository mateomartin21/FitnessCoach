using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosPerfil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PesoKg = table.Column<double>(type: "float", nullable: false),
                    EstaturaCm = table.Column<double>(type: "float", nullable: false),
                    Edad = table.Column<int>(type: "int", nullable: false),
                    ObjetivoActualTipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPerfil", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosProgreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesoKg = table.Column<double>(type: "float", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosProgreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosProgreso_UsuariosPerfil_UsuarioPerfilId",
                        column: x => x.UsuarioPerfilId,
                        principalTable: "UsuariosPerfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosProgreso_UsuarioPerfilId",
                table: "RegistrosProgreso",
                column: "UsuarioPerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosProgreso");

            migrationBuilder.DropTable(
                name: "UsuariosPerfil");
        }
    }
}
