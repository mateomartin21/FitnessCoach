using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Patterns.Strategy;

namespace FitnessCoach.Domain.Patterns.Decorator
{
    public class RutinaConEnfriamiento : RutinaDecorator
    {
        public RutinaConEnfriamiento(IEstrategiaRutina estrategia) : base(estrategia) { }
        public override Rutina GenerarRutina()
        {
            var rutina = _estrategia.GenerarRutina();
            foreach (var dia in rutina.Dias)
            {
                dia.Ejercicios.Add(new Ejercicio
                {
                    Nombre = "Enfriamiento y Estiramientos",
                    Series = 1,
                    Repeticiones = "10 min",
                    Notas = "Estiramientos estaticos de grupos musculares trabajados"
                });
            }
            return rutina;
        }
    }
}
