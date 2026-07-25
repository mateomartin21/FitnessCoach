using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DiarioDeComidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosComida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlimentoSlug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AlimentoNombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Gramos = table.Column<double>(type: "float", nullable: false),
                    Calorias = table.Column<double>(type: "float", nullable: false),
                    ProteinaG = table.Column<double>(type: "float", nullable: false),
                    CarbohidratoG = table.Column<double>(type: "float", nullable: false),
                    GrasaG = table.Column<double>(type: "float", nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosComida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosComida_UsuariosPerfil_UsuarioPerfilId",
                        column: x => x.UsuarioPerfilId,
                        principalTable: "UsuariosPerfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosComida_UsuarioPerfilId",
                table: "RegistrosComida",
                column: "UsuarioPerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosComida");
        }
    }
}
