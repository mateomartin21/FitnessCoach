using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessCoach.Domain.Models.Entrenamiento;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessCoach.Infrastructure.Data
{
    /// <summary>
    /// Puebla el catálogo desde <c>catalogo-ejercicios.json</c> la primera vez que arranca
    /// con la tabla vacía.
    ///
    /// Se prefirió un archivo de datos antes que <c>HasData</c> en una migración: agregar
    /// o corregir ejercicios pasa a ser editar un JSON, no generar una migración nueva,
    /// y el diff queda legible. El costo es que la semilla no está versionada por EF, así
    /// que reponerla exige vaciar la tabla a mano.
    /// </summary>
    public static class SembradorCatalogoEjercicios
    {
        private const string NombreArchivo = "catalogo-ejercicios.json";

        public static async Task SembrarAsync(ApplicationDbContext context, ILogger log)
        {
            if (await context.Ejercicios.AnyAsync())
                return;   // ya sembrado: no se pisa nada

            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", NombreArchivo);
            if (!File.Exists(ruta))
            {
                log.LogWarning("No se encontró {Ruta}: el catálogo de ejercicios queda vacío.", ruta);
                return;
            }

            await using var archivo = File.OpenRead(ruta);
            var crudos = await JsonSerializer.DeserializeAsync<List<EjercicioSemilla>>(
                archivo, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (crudos is null || crudos.Count == 0)
            {
                log.LogWarning("{Archivo} no contiene ejercicios.", NombreArchivo);
                return;
            }

            var ejercicios = crudos.Select(c => new Ejercicio
            {
                Slug = c.Slug,
                Nombre = c.Nombre,
                GrupoMuscular = c.GrupoMuscular,
                ParteCuerpo = c.ParteCuerpo,
                Equipo = c.Equipo,
                Categoria = c.Categoria,
                MusculosSecundarios = c.MusculosSecundarios ?? new(),
                Instrucciones = c.Instrucciones ?? new(),
                UrlGif = c.UrlGif,
                VideoYoutubeId = c.VideoYoutubeId
            });

            context.Ejercicios.AddRange(ejercicios);
            await context.SaveChangesAsync();

            log.LogInformation("Catálogo sembrado con {Cantidad} ejercicios.", crudos.Count);
        }

        /// <summary>Forma del JSON de semilla.</summary>
        private sealed class EjercicioSemilla
        {
            [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
            [JsonPropertyName("nombre")] public string Nombre { get; set; } = string.Empty;
            [JsonPropertyName("grupoMuscular")] public string GrupoMuscular { get; set; } = string.Empty;
            [JsonPropertyName("parteCuerpo")] public string ParteCuerpo { get; set; } = string.Empty;
            [JsonPropertyName("equipo")] public string Equipo { get; set; } = string.Empty;
            [JsonPropertyName("categoria")] public string Categoria { get; set; } = string.Empty;
            [JsonPropertyName("musculosSecundarios")] public List<string>? MusculosSecundarios { get; set; }
            [JsonPropertyName("instrucciones")] public List<string>? Instrucciones { get; set; }
            [JsonPropertyName("urlGif")] public string? UrlGif { get; set; }
            [JsonPropertyName("videoYoutubeId")] public string? VideoYoutubeId { get; set; }
        }
    }
}
