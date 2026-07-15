using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public interface IEstrategiaAlimentacion
    {
        PlanAlimentacion GenerarPlan();
    }
}
