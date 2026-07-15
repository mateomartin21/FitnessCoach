using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Application.Services
{
    public interface IGeneradorAlimentacion
    {
        PlanAlimentacion GenerarPlanParaObjetivo(ObjetivoFitness objetivo);
    }
}
