using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public interface IEstrategiaRutina
    {
        Rutina GenerarRutina();
    }
}
