using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Application.Services
{
    public interface IServicioDiario
    {
        /// <summary>Registra que el usuario comió esa cantidad de un alimento del catálogo, en el día dado.</summary>
        void Registrar(UsuarioPerfil usuario, string alimentoSlug, double gramos, DateOnly dia);

        /// <summary>Borra un registro del diario del usuario. Si no es suyo o no existe, no hace nada.</summary>
        void Borrar(UsuarioPerfil usuario, int registroId);

        /// <summary>Lo comido en un día frente al objetivo de macros calculado para el usuario.</summary>
        ResumenDiario ResumenDelDia(UsuarioPerfil usuario, DateOnly dia);
    }
}
