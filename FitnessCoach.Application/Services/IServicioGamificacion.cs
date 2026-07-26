using FitnessCoach.Domain.Models.Gamificacion;

namespace FitnessCoach.Application.Services
{
    public interface IServicioGamificacion
    {
        /// <summary>El estado de juego del usuario listo para mostrar: nivel, logros y misiones.</summary>
        ResumenGamificacion Obtener(string identityUserId);

        /// <summary>
        /// La foto de hechos del usuario. Sirve para comparar antes/después de registrar
        /// algo y detectar qué logros se acaban de desbloquear.
        /// </summary>
        EstadisticasUsuario Estadisticas(string identityUserId);
    }
}
