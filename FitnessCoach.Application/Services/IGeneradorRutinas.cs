using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Application.Services
{
    public interface IGeneradorRutinas
    {
        Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo);
    }
}
