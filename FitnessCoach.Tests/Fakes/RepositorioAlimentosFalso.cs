using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// Catálogo de alimentos en memoria. Escrito a mano en vez de con una librería de
    /// dobles: el puerto es chico y así las pruebas leen como código normal.
    /// </summary>
    public class RepositorioAlimentosFalso : IRepositorioAlimentos
    {
        private readonly List<Alimento> _alimentos;

        public RepositorioAlimentosFalso(params Alimento[] alimentos)
        {
            _alimentos = alimentos.ToList();
        }

        /// <summary>Catálogo mínimo pero suficiente para componer una comida completa.</summary>
        public static RepositorioAlimentosFalso ConCatalogoDePrueba() => new(
            new Alimento
            {
                Slug = "pechuga-de-pollo", Nombre = "Pechuga de pollo",
                Categoria = "proteina", GrupoIntercambio = "proteina-magra",
                ProteinaPor100g = 22.5, CarbohidratoPor100g = 0, GrasaPor100g = 2.62,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 250,
                DescripcionPorcion = "1 pechuga mediana",
                EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" }
            },
            new Alimento
            {
                Slug = "tofu", Nombre = "Tofu firme",
                Categoria = "proteina", GrupoIntercambio = "proteina-vegetal",
                ProteinaPor100g = 17.3, CarbohidratoPor100g = 2.78, GrasaPor100g = 8.72,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 250,
                DescripcionPorcion = "1 bloque pequeño",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" }
            },
            new Alimento
            {
                Slug = "atun-en-agua", Nombre = "Atún en agua",
                Categoria = "proteina", GrupoIntercambio = "proteina-magra",
                ProteinaPor100g = 25.5, CarbohidratoPor100g = 0, GrasaPor100g = 0.82,
                PorcionTipicaG = 100, PorcionMinimaG = 60, PorcionMaximaG = 180,
                DescripcionPorcion = "1 lata escurrida",
                EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" }
            },
            new Alimento
            {
                Slug = "arroz-integral", Nombre = "Arroz integral cocido",
                Categoria = "carbohidrato", GrupoIntercambio = "cereal",
                ProteinaPor100g = 2.74, CarbohidratoPor100g = 25.6, GrasaPor100g = 0.97,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 300,
                DescripcionPorcion = "1 taza cocido",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" }
            },
            new Alimento
            {
                Slug = "brocoli", Nombre = "Brócoli",
                Categoria = "verdura", GrupoIntercambio = "verdura",
                ProteinaPor100g = 2.82, CarbohidratoPor100g = 6.64, GrasaPor100g = 0.37,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 300,
                DescripcionPorcion = "1 taza",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" }
            },
            new Alimento
            {
                Slug = "aceite-de-oliva", Nombre = "Aceite de oliva",
                Categoria = "grasa", GrupoIntercambio = "grasa",
                ProteinaPor100g = 0, CarbohidratoPor100g = 0, GrasaPor100g = 100,
                PorcionTipicaG = 14, PorcionMinimaG = 5, PorcionMaximaG = 30,
                DescripcionPorcion = "1 cucharada",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" }
            });

        public IReadOnlyList<Alimento> ObtenerTodos() => _alimentos;

        public Alimento? ObtenerPorSlug(string slug) =>
            _alimentos.FirstOrDefault(a => a.Slug == slug);

        public IReadOnlyList<Alimento> ObtenerPorCategoria(string categoria) =>
            _alimentos.Where(a => a.Categoria == categoria).ToList();

        public IReadOnlyList<Alimento> ObtenerPorGrupoIntercambio(string grupoIntercambio) =>
            _alimentos.Where(a => a.GrupoIntercambio == grupoIntercambio).ToList();
    }
}
