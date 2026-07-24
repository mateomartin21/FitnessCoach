using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public interface IServicioProgreso
    {
        /// <summary>Historial del usuario, del más reciente al más antiguo.</summary>
        IReadOnlyList<RegistroProgreso> ObtenerHistorial(string identityUserId);

        RegistroProgreso Agregar(string identityUserId, double pesoKg, string? notas);

        /// <summary>False si el registro no existe o no es de este usuario.</summary>
        bool Editar(string identityUserId, int registroId, double pesoKg, string? notas);

        /// <summary>False si el registro no existe o no es de este usuario.</summary>
        bool Eliminar(string identityUserId, int registroId);

        RegistroProgreso? ObtenerRegistro(string identityUserId, int registroId);
    }
}
