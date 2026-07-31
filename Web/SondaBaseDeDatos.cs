using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FitnessCoach.Web
{
    /// <summary>
    /// Responde a <c>/health</c>. La app sin base no sirve para nada, así que la sonda
    /// no se limita a decir "el proceso vive": abre una conexión de verdad.
    /// </summary>
    public class SondaBaseDeDatos : IHealthCheck
    {
        private readonly ApplicationDbContext _contexto;

        public SondaBaseDeDatos(ApplicationDbContext contexto) => _contexto = contexto;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext contexto, CancellationToken token = default)
        {
            try
            {
                return await _contexto.Database.CanConnectAsync(token)
                    ? HealthCheckResult.Healthy("La base responde.")
                    : HealthCheckResult.Unhealthy("La base no acepta conexiones.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Error al conectar con la base.", ex);
            }
        }
    }
}
