using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Patterns.Strategy;

namespace FitnessCoach.Domain.Patterns.Decorator
{
    public class RutinaConCalentamiento : RutinaDecorator
    {
        public RutinaConCalentamiento(IEstrategiaRutina estrategia) : base(estrategia) { }
        public override Rutina GenerarRutina()
        {
            var rutina = _estrategia.GenerarRutina();
            foreach (var dia in rutina.Dias)
            {
                dia.Ejercicios.Insert(0, new Ejercicio
                {
                    Nombre = "Calentamiento General",
                    Series = 1,
                    Repeticiones = "10 min",
                    Notas = "Movilidad articular + cardio ligero"
                });
            }
            return rutina;
        }
    }
}
