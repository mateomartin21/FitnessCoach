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
                EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "tofu", Nombre = "Tofu firme",
                Categoria = "proteina", GrupoIntercambio = "proteina-vegetal",
                ProteinaPor100g = 17.3, CarbohidratoPor100g = 2.78, GrasaPor100g = 8.72,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 250,
                DescripcionPorcion = "1 bloque pequeño",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "atun-en-agua", Nombre = "Atún en agua",
                Categoria = "proteina", GrupoIntercambio = "proteina-magra",
                ProteinaPor100g = 25.5, CarbohidratoPor100g = 0, GrasaPor100g = 0.82,
                PorcionTipicaG = 100, PorcionMinimaG = 60, PorcionMaximaG = 180,
                DescripcionPorcion = "1 lata escurrida",
                EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal", "snack" }
            },
            new Alimento
            {
                Slug = "arroz-integral", Nombre = "Arroz integral cocido",
                Categoria = "carbohidrato", GrupoIntercambio = "cereal",
                ProteinaPor100g = 2.74, CarbohidratoPor100g = 25.6, GrasaPor100g = 0.97,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 300,
                DescripcionPorcion = "1 taza cocido",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "brocoli", Nombre = "Brócoli",
                Categoria = "verdura", GrupoIntercambio = "verdura",
                ProteinaPor100g = 2.82, CarbohidratoPor100g = 6.64, GrasaPor100g = 0.37,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 300,
                DescripcionPorcion = "1 taza",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "aceite-de-oliva", Nombre = "Aceite de oliva",
                Categoria = "grasa", GrupoIntercambio = "grasa",
                ProteinaPor100g = 0, CarbohidratoPor100g = 0, GrasaPor100g = 100,
                PorcionTipicaG = 14, PorcionMinimaG = 5, PorcionMaximaG = 30,
                DescripcionPorcion = "1 cucharada",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "huevo-entero", Nombre = "Huevo entero",
                Categoria = "proteina", GrupoIntercambio = "proteina-media",
                ProteinaPor100g = 12.6, CarbohidratoPor100g = 0.72, GrasaPor100g = 9.51,
                PorcionTipicaG = 100, PorcionMinimaG = 50, PorcionMaximaG = 200,
                DescripcionPorcion = "2 huevos medianos",
                EtiquetasDieta = new List<string> { "vegetariano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "desayuno", "principal", "snack" }
            },
            new Alimento
            {
                Slug = "salmon", Nombre = "Salmón",
                Categoria = "proteina", GrupoIntercambio = "pescado-graso",
                ProteinaPor100g = 20.4, CarbohidratoPor100g = 0, GrasaPor100g = 13.4,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 220,
                DescripcionPorcion = "1 filete",
                EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "avena", Nombre = "Avena en hojuelas",
                Categoria = "carbohidrato", GrupoIntercambio = "cereal",
                ProteinaPor100g = 13.2, CarbohidratoPor100g = 67.7, GrasaPor100g = 6.52,
                PorcionTipicaG = 60, PorcionMinimaG = 30, PorcionMaximaG = 120,
                DescripcionPorcion = "3/4 taza en seco",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-lactosa" },
                MomentosAptos = new List<string> { "desayuno", "snack" }
            },
            new Alimento
            {
                Slug = "papa-cocida", Nombre = "Papa cocida",
                Categoria = "carbohidrato", GrupoIntercambio = "tuberculo",
                ProteinaPor100g = 2.05, CarbohidratoPor100g = 17.5, GrasaPor100g = 0.09,
                PorcionTipicaG = 200, PorcionMinimaG = 100, PorcionMaximaG = 350,
                DescripcionPorcion = "1 papa grande",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "banana", Nombre = "Banana",
                Categoria = "fruta", GrupoIntercambio = "fruta",
                ProteinaPor100g = 1.09, CarbohidratoPor100g = 22.8, GrasaPor100g = 0.33,
                PorcionTipicaG = 120, PorcionMinimaG = 70, PorcionMaximaG = 250,
                DescripcionPorcion = "1 banana mediana",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "desayuno", "principal", "snack" }
            },
            new Alimento
            {
                Slug = "manzana", Nombre = "Manzana",
                Categoria = "fruta", GrupoIntercambio = "fruta",
                ProteinaPor100g = 0.26, CarbohidratoPor100g = 13.8, GrasaPor100g = 0.17,
                PorcionTipicaG = 180, PorcionMinimaG = 100, PorcionMaximaG = 300,
                DescripcionPorcion = "1 manzana mediana",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "desayuno", "principal", "snack" }
            },
            new Alimento
            {
                Slug = "yogur-griego", Nombre = "Yogur griego natural",
                Categoria = "lacteo", GrupoIntercambio = "lacteo-proteico",
                ProteinaPor100g = 9.53, CarbohidratoPor100g = 3.43, GrasaPor100g = 0.24,
                PorcionTipicaG = 170, PorcionMinimaG = 100, PorcionMaximaG = 300,
                DescripcionPorcion = "1 pote",
                EtiquetasDieta = new List<string> { "vegetariano", "sin-gluten" },
                MomentosAptos = new List<string> { "desayuno", "principal", "snack" }
            },
            new Alimento
            {
                Slug = "queso-cottage", Nombre = "Queso cottage",
                Categoria = "lacteo", GrupoIntercambio = "lacteo-proteico",
                ProteinaPor100g = 10.4, CarbohidratoPor100g = 4.76, GrasaPor100g = 2.27,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 250,
                DescripcionPorcion = "3/4 taza",
                EtiquetasDieta = new List<string> { "vegetariano", "sin-gluten" },
                MomentosAptos = new List<string> { "desayuno", "principal", "snack" }
            },
            new Alimento
            {
                Slug = "almendras", Nombre = "Almendras",
                Categoria = "grasa", GrupoIntercambio = "frutos-secos",
                ProteinaPor100g = 21, CarbohidratoPor100g = 21, GrasaPor100g = 52.5,
                PorcionTipicaG = 30, PorcionMinimaG = 15, PorcionMaximaG = 60,
                DescripcionPorcion = "1 puñado",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "desayuno", "snack" }
            },
            new Alimento
            {
                Slug = "espinaca", Nombre = "Espinaca",
                Categoria = "verdura", GrupoIntercambio = "verdura",
                ProteinaPor100g = 2.86, CarbohidratoPor100g = 3.63, GrasaPor100g = 0.39,
                PorcionTipicaG = 100, PorcionMinimaG = 50, PorcionMaximaG = 250,
                DescripcionPorcion = "2 tazas crudas",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal" }
            },
            new Alimento
            {
                Slug = "zanahoria", Nombre = "Zanahoria",
                Categoria = "verdura", GrupoIntercambio = "verdura",
                ProteinaPor100g = 0.93, CarbohidratoPor100g = 9.58, GrasaPor100g = 0.24,
                PorcionTipicaG = 120, PorcionMinimaG = 60, PorcionMaximaG = 250,
                DescripcionPorcion = "2 zanahorias",
                EtiquetasDieta = new List<string> { "vegetariano", "vegano", "sin-gluten", "sin-lactosa" },
                MomentosAptos = new List<string> { "principal", "snack" }
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
