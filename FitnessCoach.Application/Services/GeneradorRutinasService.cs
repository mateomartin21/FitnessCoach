using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Patterns.Strategy;
using FitnessCoach.Domain.Patterns.Decorator;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    public class GeneradorRutinasService : IGeneradorRutinas
    {
        private readonly IRepositorioEjercicios _catalogo;

        public GeneradorRutinasService(IRepositorioEjercicios catalogo)
        {
            _catalogo = catalogo;
        }

        public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo, int semillaRotacion = 0)
        {
            // PATRÓN STRATEGY — selecciona la estrategia según el objetivo.
            // Cada una recibe el catálogo: ya no trae los ejercicios adentro.
            IEstrategiaRutina estrategia = objetivo switch
            {
                ObjetivoPerderPeso => new EstrategiaPerderPeso(_catalogo, semillaRotacion),
                ObjetivoGanarMusculo => new EstrategiaGanarMusculo(_catalogo, semillaRotacion),
                ObjetivoRecomposicion => new EstrategiaRecomposicion(_catalogo, semillaRotacion),
                _ => new EstrategiaRecomposicion(_catalogo, semillaRotacion)
            };

            // PATRÓN DECORATOR — envuelve la estrategia con calentamiento y enfriamiento
            IEstrategiaRutina rutinaDecorada = new RutinaConEnfriamiento(
                                               new RutinaConCalentamiento(estrategia));

            return rutinaDecorada.GenerarRutina();
        }
    }
}
