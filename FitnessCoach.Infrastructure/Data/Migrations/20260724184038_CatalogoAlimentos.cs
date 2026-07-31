using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoAlimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NombreIngles = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GrupoIntercambio = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProteinaPor100g = table.Column<double>(type: "float", nullable: false),
                    CarbohidratoPor100g = table.Column<double>(type: "float", nullable: false),
                    GrasaPor100g = table.Column<double>(type: "float", nullable: false),
                    FibraPor100g = table.Column<double>(type: "float", nullable: false),
                    PorcionTipicaG = table.Column<double>(type: "float", nullable: false),
                    DescripcionPorcion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PorcionMinimaG = table.Column<double>(type: "float", nullable: false),
                    PorcionMaximaG = table.Column<double>(type: "float", nullable: false),
                    EtiquetasDieta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlImagen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AutorImagen = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LicenciaImagen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FdcId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alimentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_Categoria",
                table: "Alimentos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_GrupoIntercambio",
                table: "Alimentos",
                column: "GrupoIntercambio");

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_Slug",
                table: "Alimentos",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alimentos");
        }
    }
}
