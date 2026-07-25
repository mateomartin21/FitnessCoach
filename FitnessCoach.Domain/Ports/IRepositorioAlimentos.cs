using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Puerto del catálogo de alimentos. Las estrategias de alimentación componen los
    /// planes desde acá en vez de tener las comidas escritas a mano, igual que las
    /// rutinas hacen con <see cref="IRepositorioEjercicios"/>.
    /// </summary>
    public interface IRepositorioAlimentos
    {
        IReadOnlyList<Alimento> ObtenerTodos();

        Alimento? ObtenerPorSlug(string slug);

        /// <summary>Alimentos de una categoría culinaria (ej. "proteina", "verdura").</summary>
        IReadOnlyList<Alimento> ObtenerPorCategoria(string categoria);

        /// <summary>
        /// Alimentos que cumplen el mismo papel nutricional y por lo tanto pueden
        /// sustituirse entre sí. Es la base del sistema de equivalencias.
        /// </summary>
        IReadOnlyList<Alimento> ObtenerPorGrupoIntercambio(string grupoIntercambio);
    }
}
