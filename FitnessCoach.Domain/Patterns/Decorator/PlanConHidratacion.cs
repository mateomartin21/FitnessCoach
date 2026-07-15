using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Decorator
{
    public class PlanConHidratacion : IEstrategiaAlimentacion
    {
        private readonly IEstrategiaAlimentacion _estrategia;
        public PlanConHidratacion(IEstrategiaAlimentacion estrategia)
        {
            _estrategia = estrategia;
        }
        public PlanAlimentacion GenerarPlan()
        {
            var plan = _estrategia.GenerarPlan();
            plan.RecomendacionesGenerales.Add("Tomar 500ml de agua al despertar en ayunas");
            plan.RecomendacionesGenerales.Add("Beber 250ml de agua 30 minutos antes de cada comida");
            plan.RecomendacionesGenerales.Add("Evitar bebidas azucaradas y alcohol durante el plan");
            return plan;
        }
    }
}
