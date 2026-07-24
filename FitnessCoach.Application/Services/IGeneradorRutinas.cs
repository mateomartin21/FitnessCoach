using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Application.Services
{
    public interface IGeneradorRutinas
    {
        /// <summary>
        /// Compone una rutina para el objetivo tomando ejercicios del catálogo.
        /// </summary>
        /// <param name="semillaRotacion">
        /// Hace que dos personas con el mismo objetivo no reciban los mismos ejercicios,
        /// manteniendo estable la rutina de cada una. Se le suele pasar el Id del perfil.
        /// </param>
        Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo, int semillaRotacion = 0);
    }
}
