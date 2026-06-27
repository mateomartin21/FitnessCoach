using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public interface ICalculadorCalorico
    {
        double CalcularCaloriasDiarias(UsuarioPerfil usuario);
    }
}
