using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que el formulario de preferencias envía y muestra. Las dietas y los alimentos
    /// excluidos llegan como listas de valores marcados; el catálogo se usa solo para
    /// dibujar las opciones y no viaja de vuelta en el POST.
    /// </summary>
    public class PreferenciasViewModel
    {
        /// <summary>Etiquetas de dieta marcadas: "vegetariano", "vegano", "sin-gluten", "sin-lactosa".</summary>
        public List<string> Dietas { get; set; } = new();

        /// <summary>Slugs de los alimentos que el usuario excluyó.</summary>
        public List<string> AlimentosExcluidos { get; set; } = new();

        /// <summary>Las dietas ofrecidas, con su etiqueta técnica y su nombre visible.</summary>
        public static readonly IReadOnlyList<(string Valor, string Titulo, string Detalle)> DietasDisponibles = new[]
        {
            ("vegetariano", "Vegetariano", "Sin carne ni pescado"),
            ("vegano", "Vegano", "Sin ningún producto de origen animal"),
            ("sin-gluten", "Sin gluten", "Apto para celiaquía o sensibilidad"),
            ("sin-lactosa", "Sin lactosa", "Sin lácteos que la contengan")
        };

        /// <summary>El catálogo agrupado por categoría, solo para dibujar los checkboxes de exclusión.</summary>
        public IReadOnlyList<IGrouping<string, Alimento>> CatalogoPorCategoria { get; set; } =
            Array.Empty<IGrouping<string, Alimento>>();
    }
}
