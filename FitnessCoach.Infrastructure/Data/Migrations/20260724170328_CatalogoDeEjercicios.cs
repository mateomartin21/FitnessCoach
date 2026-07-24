using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoDeEjercicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ParteCuerpo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Equipo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MusculosSecundarios = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instrucciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlGif = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoYoutubeId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_Equipo",
                table: "Ejercicios",
                column: "Equipo");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_GrupoMuscular",
                table: "Ejercicios",
                column: "GrupoMuscular");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_Slug",
                table: "Ejercicios",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ejercicios");
        }
    }
}
