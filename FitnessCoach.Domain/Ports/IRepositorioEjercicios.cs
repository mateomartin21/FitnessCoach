using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Puerto del catálogo de ejercicios. Las estrategias componen rutinas desde acá
    /// en vez de tener los ejercicios incrustados.
    /// </summary>
    public interface IRepositorioEjercicios
    {
        IReadOnlyList<Ejercicio> ObtenerTodos();

        Ejercicio? ObtenerPorSlug(string slug);

        /// <summary>Ejercicios de un músculo concreto (ej. "biceps").</summary>
        IReadOnlyList<Ejercicio> ObtenerPorGrupoMuscular(string grupoMuscular);
    }
}
