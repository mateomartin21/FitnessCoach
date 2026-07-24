using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessCoach.Domain.Models.Alimentacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessCoach.Infrastructure.Data
{
    /// <summary>
    /// Puebla el catálogo de alimentos desde <c>catalogo-alimentos.json</c> la primera
    /// vez que arranca con la tabla vacía. Mismo criterio que el catálogo de ejercicios:
    /// un archivo de datos en vez de <c>HasData</c>, para que corregir un alimento sea
    /// editar un JSON y no generar una migración.
    ///
    /// Origen de los datos:
    ///   - Macros: USDA FoodData Central, volcado SR Legacy (dominio público).
    ///     https://fdc.nal.usda.gov/fdc-datasets/FoodData_Central_sr_legacy_food_json_2018-04.zip
    ///   - Imágenes: Wikimedia Commons vía Wikipedia en español, con el autor y la
    ///     licencia de cada foto guardados junto a la URL.
    ///
    /// La lista de qué alimentos entran es curada a mano (<c>lista-alimentos-curada.json</c>):
    /// USDA tiene cientos de miles de entradas, en su mayoría productos de marca que
    /// harían el plan peor, no mejor.
    /// </summary>
    public static class SembradorCatalogoAlimentos
    {
        private const string NombreArchivo = "catalogo-alimentos.json";

        public static async Task SembrarAsync(ApplicationDbContext context, ILogger log)
        {
            if (await context.Alimentos.AnyAsync())
                return;   // ya sembrado: no se pisa nada

            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", NombreArchivo);
            if (!File.Exists(ruta))
            {
                log.LogWarning("No se encontró {Ruta}: el catálogo de alimentos queda vacío.", ruta);
                return;
            }

            await using var archivo = File.OpenRead(ruta);
            var crudos = await JsonSerializer.DeserializeAsync<List<AlimentoSemilla>>(
                archivo, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (crudos is null || crudos.Count == 0)
            {
                log.LogWarning("{Archivo} no contiene alimentos.", NombreArchivo);
                return;
            }

            var alimentos = crudos.Select(c => new Alimento
            {
                Slug = c.Slug,
                Nombre = c.Nombre,
                NombreIngles = c.NombreIngles,
                FdcId = c.FdcId,
                Categoria = c.Categoria,
                GrupoIntercambio = c.GrupoIntercambio,
                ProteinaPor100g = c.ProteinaPor100g,
                CarbohidratoPor100g = c.CarbohidratoPor100g,
                GrasaPor100g = c.GrasaPor100g,
                FibraPor100g = c.FibraPor100g,
                PorcionTipicaG = c.PorcionTipicaG,
                DescripcionPorcion = c.DescripcionPorcion,
                PorcionMinimaG = c.PorcionMinimaG,
                PorcionMaximaG = c.PorcionMaximaG,
                EtiquetasDieta = c.EtiquetasDieta ?? new(),
                UrlImagen = c.UrlImagen,
                AutorImagen = c.AutorImagen,
                LicenciaImagen = c.LicenciaImagen
            });

            context.Alimentos.AddRange(alimentos);
            await context.SaveChangesAsync();

            log.LogInformation("Catálogo sembrado con {Cantidad} alimentos.", crudos.Count);
        }

        /// <summary>Forma del JSON de semilla.</summary>
        private sealed class AlimentoSemilla
        {
            [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
            [JsonPropertyName("nombre")] public string Nombre { get; set; } = string.Empty;
            [JsonPropertyName("nombreIngles")] public string NombreIngles { get; set; } = string.Empty;
            [JsonPropertyName("fdcId")] public int? FdcId { get; set; }
            [JsonPropertyName("categoria")] public string Categoria { get; set; } = string.Empty;
            [JsonPropertyName("grupoIntercambio")] public string GrupoIntercambio { get; set; } = string.Empty;
            [JsonPropertyName("proteinaPor100g")] public double ProteinaPor100g { get; set; }
            [JsonPropertyName("carbohidratoPor100g")] public double CarbohidratoPor100g { get; set; }
            [JsonPropertyName("grasaPor100g")] public double GrasaPor100g { get; set; }
            [JsonPropertyName("fibraPor100g")] public double FibraPor100g { get; set; }
            [JsonPropertyName("porcionTipicaG")] public double PorcionTipicaG { get; set; }
            [JsonPropertyName("descripcionPorcion")] public string DescripcionPorcion { get; set; } = string.Empty;
            [JsonPropertyName("porcionMinimaG")] public double PorcionMinimaG { get; set; }
            [JsonPropertyName("porcionMaximaG")] public double PorcionMaximaG { get; set; }
            [JsonPropertyName("etiquetasDieta")] public List<string>? EtiquetasDieta { get; set; }
            [JsonPropertyName("urlImagen")] public string? UrlImagen { get; set; }
            [JsonPropertyName("autorImagen")] public string? AutorImagen { get; set; }
            [JsonPropertyName("licenciaImagen")] public string? LicenciaImagen { get; set; }
        }
    }
}
