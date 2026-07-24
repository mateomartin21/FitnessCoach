using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Patterns.Strategy;
using FitnessCoach.Domain.Patterns.Decorator;

namespace FitnessCoach.Application.Services
{
    public class GeneradorRutinasService : IGeneradorRutinas
    {
        public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo)
        {
            // PATRÓN STRATEGY — selecciona la estrategia según el objetivo
            IEstrategiaRutina estrategia = objetivo switch
            {
                ObjetivoPerderPeso => new EstrategiaPerderPeso(),
                ObjetivoGanarMusculo => new EstrategiaGanarMusculo(),
                ObjetivoRecomposicion => new EstrategiaRecomposicion(),
                _ => new EstrategiaRecomposicion()
            };

            // PATRÓN DECORATOR — envuelve la estrategia con calentamiento y enfriamiento
            IEstrategiaRutina rutinaDecorada = new RutinaConEnfriamiento(
                                               new RutinaConCalentamiento(estrategia));

            return rutinaDecorada.GenerarRutina();
        }
    }
}