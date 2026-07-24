using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public interface IEstrategiaAlimentacion
    {
        /// <summary>
        /// Arma el plan para unos macros concretos. Recibirlos por parámetro es lo que
        /// hace que el plan sea del usuario: antes cada estrategia devolvía siempre las
        /// mismas comidas y las mismas calorías fijas, pesara quien pesara.
        /// </summary>
        PlanAlimentacion GenerarPlan(ObjetivoMacros macrosDiarios);
    }
}
