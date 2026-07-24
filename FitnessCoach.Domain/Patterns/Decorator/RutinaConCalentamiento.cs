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
                // El calentamiento no sale del catálogo: es una consigna genérica, no un
                // ejercicio concreto con GIF y grupo muscular.
                dia.Ejercicios.Insert(0, new EjercicioPrescrito
                {
                    Ejercicio = new Ejercicio { Slug = "calentamiento-general", Nombre = "Calentamiento General" },
                    Series = 1,
                    Repeticiones = "10 min",
                    Notas = "Movilidad articular + cardio ligero"
                });
            }
            return rutina;
        }
    }
}
