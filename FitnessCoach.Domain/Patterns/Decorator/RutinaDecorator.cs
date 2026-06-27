using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Patterns.Strategy;

namespace FitnessCoach.Domain.Patterns.Decorator
{
    public abstract class RutinaDecorator : IEstrategiaRutina
    {
        protected readonly IEstrategiaRutina _estrategia;
        protected RutinaDecorator(IEstrategiaRutina estrategia)
        {
            _estrategia = estrategia;
        }
        public virtual Rutina GenerarRutina() => _estrategia.GenerarRutina();
    }
}
