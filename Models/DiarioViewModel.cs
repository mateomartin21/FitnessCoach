using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Models
{
    /// <summary>Lo que la pantalla del diario necesita: el resumen del día, el plan para
    /// registrar comidas de un toque, y el catálogo para agregar cualquier alimento.</summary>
    public class DiarioViewModel
    {
        public DateOnly Dia { get; set; }
        public required ResumenDiario Resumen { get; set; }

        /// <summary>El plan del usuario, para el botón "registrar esta comida".</summary>
        public PlanAlimentacion? Plan { get; set; }

        /// <summary>El catálogo agrupado por categoría, para el selector de "agregar alimento".</summary>
        public IReadOnlyList<IGrouping<string, Alimento>> CatalogoPorCategoria { get; set; } =
            Array.Empty<IGrouping<string, Alimento>>();

        public bool EsHoy => Dia == DateOnly.FromDateTime(DateTime.Today);
    }
}
