using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FitnessCoach.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EsquemaInicialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NombreIngles = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GrupoIntercambio = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProteinaPor100g = table.Column<double>(type: "double precision", nullable: false),
                    CarbohidratoPor100g = table.Column<double>(type: "double precision", nullable: false),
                    GrasaPor100g = table.Column<double>(type: "double precision", nullable: false),
                    FibraPor100g = table.Column<double>(type: "double precision", nullable: false),
                    PorcionTipicaG = table.Column<double>(type: "double precision", nullable: false),
                    DescripcionPorcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PorcionMinimaG = table.Column<double>(type: "double precision", nullable: false),
                    PorcionMaximaG = table.Column<double>(type: "double precision", nullable: false),
                    EtiquetasDieta = table.Column<string>(type: "text", nullable: false),
                    MomentosAptos = table.Column<string>(type: "text", nullable: false),
                    UrlImagen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AutorImagen = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LicenciaImagen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FdcId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alimentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ParteCuerpo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Equipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    MusculosSecundarios = table.Column<string>(type: "text", nullable: false),
                    Instrucciones = table.Column<string>(type: "text", nullable: false),
                    UrlGif = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VideoYoutubeId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPerfil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityUserId = table.Column<string>(type: "text", nullable: true),
                    ZonaHoraria = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PesoKg = table.Column<double>(type: "double precision", nullable: false),
                    EstaturaCm = table.Column<double>(type: "double precision", nullable: false),
                    Edad = table.Column<int>(type: "integer", nullable: false),
                    ObjetivoActualTipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Preferencias_DietasSeguidas = table.Column<string>(type: "text", nullable: false),
                    Preferencias_AlimentosExcluidos = table.Column<string>(type: "text", nullable: false),
                    PreferenciasEntrenamiento_EquipoDisponible = table.Column<string>(type: "text", nullable: false),
                    PreferenciasEntrenamiento_Sustituciones = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPerfil", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntrenamientosCompletados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NombreRutina = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DuracionMinutos = table.Column<int>(type: "integer", nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RecordsPersonales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EjercicioSlug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EjercicioNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PesoKg = table.Column<double>(type: "double precision", nullable: false),
                    Repeticiones = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RegistrosComida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AlimentoSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AlimentoNombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Gramos = table.Column<double>(type: "double precision", nullable: false),
                    Calorias = table.Column<double>(type: "double precision", nullable: false),
                    ProteinaG = table.Column<double>(type: "double precision", nullable: false),
                    CarbohidratoG = table.Column<double>(type: "double precision", nullable: false),
                    GrasaG = table.Column<double>(type: "double precision", nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RegistrosProgreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PesoKg = table.Column<double>(type: "double precision", nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UsuarioPerfilId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientosCompletados_UsuarioPerfilId",
                table: "EntrenamientosCompletados",
                column: "UsuarioPerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordsPersonales_UsuarioPerfilId_EjercicioSlug",
                table: "RecordsPersonales",
                columns: new[] { "UsuarioPerfilId", "EjercicioSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosComida_UsuarioPerfilId",
                table: "RegistrosComida",
                column: "UsuarioPerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosProgreso_UsuarioPerfilId",
                table: "RegistrosProgreso",
                column: "UsuarioPerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPerfil_IdentityUserId",
                table: "UsuariosPerfil",
                column: "IdentityUserId",
                unique: true,
                filter: "\"IdentityUserId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alimentos");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "EntrenamientosCompletados");

            migrationBuilder.DropTable(
                name: "RecordsPersonales");

            migrationBuilder.DropTable(
                name: "RegistrosComida");

            migrationBuilder.DropTable(
                name: "RegistrosProgreso");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UsuariosPerfil");
        }
    }
}
