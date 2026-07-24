using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecordsPersonales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecordsPersonales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioSlug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EjercicioNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PesoKg = table.Column<double>(type: "float", nullable: false),
                    Repeticiones = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordsPersonales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordsPersonales_UsuariosPerfil_UsuarioPerfilId",
                        column: x => x.UsuarioPerfilId,
                        principalTable: "UsuariosPerfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordsPersonales_UsuarioPerfilId_EjercicioSlug",
                table: "RecordsPersonales",
                columns: new[] { "UsuarioPerfilId", "EjercicioSlug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordsPersonales");
        }
    }
}
