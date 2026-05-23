using FitnessCoach.Models;

namespace FitnessCoach.Services
{
    public interface ICalculadorCalorico
    {
        double CalcularCaloriasDiarias(UsuarioPerfil usuario);
    }
}
