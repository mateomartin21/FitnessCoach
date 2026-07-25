using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Application.Services
{
    public interface IGeneradorAlimentacion
    {
        /// <summary>
        /// Genera el plan para un usuario concreto. Recibe el perfil entero y no solo
        /// el objetivo porque las porciones dependen del peso: el objetivo dice cuánta
        /// proteína por kilo, el peso dice cuántos kilos.
        /// </summary>
        PlanAlimentacion GenerarPlanPara(UsuarioPerfil usuario);
    }
}
