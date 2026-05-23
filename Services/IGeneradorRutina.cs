using FitnessCoach.Models.Entrenamiento;
using FitnessCoach.Models.Objetivos;

namespace FitnessCoach.Services
{
    public interface IGeneradorRutinas
    {
        Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo);
    }
}