using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Cambiar un ejercicio de la rutina por otro que trabaje lo mismo. El equivalente
    /// para entrenamiento de lo que <c>CalculadorEquivalencias</c> hace con la comida.
    /// </summary>
    public interface IServicioSustitucionEjercicios
    {
        /// <summary>El que se está viendo hoy en la fila cuyo original es <paramref name="slugReferencia"/>.</summary>
        Ejercicio? EnUso(UsuarioPerfil usuario, string slugReferencia);

        /// <summary>
        /// Ejercicios del mismo grupo muscular que el usuario puede hacer con su equipo,
        /// sin el que ya está en uso. Un grupo grande deja más de cien, así que
        /// <paramref name="busqueda"/> filtra por nombre.
        /// </summary>
        IReadOnlyList<Ejercicio> Alternativas(UsuarioPerfil usuario, string slugReferencia, string? busqueda = null);

        /// <summary>
        /// Registra el cambio. Devuelve false si el elegido no existe o no trabaja el mismo
        /// grupo muscular: el usuario cambia un ejercicio, no la rutina.
        /// </summary>
        bool Sustituir(UsuarioPerfil usuario, string slugReferencia, string slugElegido);

        /// <summary>Deshace el cambio y devuelve el que había elegido la estrategia.</summary>
        void Restaurar(UsuarioPerfil usuario, string slugReferencia);
    }
}
