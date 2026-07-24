using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    /// <summary>Resultado de intentar registrar una marca.</summary>
    /// <param name="EsNuevoRecord">Si superó la marca anterior.</param>
    /// <param name="Record">La marca vigente tras el intento.</param>
    /// <param name="MejoraKg">Cuánto peso se superó respecto del récord anterior. Null si es el primero.</param>
    public readonly record struct ResultadoRecord(bool EsNuevoRecord, RecordPersonal Record, double? MejoraKg);

    public interface IServicioRecords
    {
        /// <summary>Récords del usuario, del más reciente al más antiguo.</summary>
        IReadOnlyList<RecordPersonal> ObtenerTodos(string identityUserId);

        RecordPersonal? ObtenerDeEjercicio(string identityUserId, string ejercicioSlug);

        /// <summary>
        /// Registra una marca. Solo se guarda si supera la anterior de ese ejercicio.
        /// </summary>
        ResultadoRecord Registrar(string identityUserId, string ejercicioSlug, string ejercicioNombre,
                                  double pesoKg, int repeticiones);

        /// <summary>False si el récord no existe o no es de este usuario.</summary>
        bool Eliminar(string identityUserId, int recordId);
    }
}
