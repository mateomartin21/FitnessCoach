using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using FitnessCoach.Domain.Patterns.Decorator;

namespace FitnessCoach.Application.Services
{
    public class GeneradorAlimentacionService : IGeneradorAlimentacion
    {
        public PlanAlimentacion GenerarPlanParaObjetivo(ObjetivoFitness objetivo)
        {
            IEstrategiaAlimentacion estrategia = objetivo switch
            {
                ObjetivoPerderPeso    => new AlimentacionPerderPeso(),
                ObjetivoGanarMusculo  => new AlimentacionGanarMusculo(),
                ObjetivoRecomposicion => new AlimentacionRecomposicion(),
                _                     => new AlimentacionRecomposicion()
            };

            return new PlanConHidratacion(estrategia).GenerarPlan();
        }
    }
}
