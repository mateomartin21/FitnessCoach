using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public interface IServicioEntrenamientos
    {
        /// <summary>Entrenamientos del usuario, del más reciente al más antiguo.</summary>
        IReadOnlyList<EntrenamientoCompletado> ObtenerHistorial(string identityUserId);

        /// <summary>
        /// Únicas etiquetas válidas para registrar un entrenamiento. Vacío si no tiene objetivo.
        /// </summary>
        IReadOnlyList<string> OpcionesDeRutina(string identityUserId);

        /// <summary>
        /// <paramref name="nombreRutina"/> tiene que ser uno de <see cref="OpcionesDeRutina"/>;
        /// si no, lanza <see cref="ArgumentException"/>.
        /// </summary>
        EntrenamientoCompletado Registrar(string identityUserId, string nombreRutina, int duracionMinutos, string? notas);

        /// <summary>False si el entrenamiento no existe o no es de este usuario.</summary>
        bool Eliminar(string identityUserId, int entrenamientoId);

        Rachas ObtenerRachas(string identityUserId);
    }
}
