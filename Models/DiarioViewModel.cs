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

        /// <summary>
        /// El día de hoy EN LA ZONA DEL USUARIO. Lo provee el controlador, que es quien
        /// tiene el perfil: la vista no puede preguntarle la hora al servidor porque su
        /// medianoche no es la del usuario (D-25).
        /// </summary>
        public DateOnly Hoy { get; set; }

        public bool EsHoy => Dia == Hoy;
    }
}
