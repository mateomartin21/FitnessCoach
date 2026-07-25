using FitnessCoach.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// El coach que no se muere. Arma el prompt una sola vez y prueba los proveedores
    /// de IA en orden: usa la respuesta del primero que conteste. Si uno falla, registra
    /// el fallo y pasa al siguiente (Chain of Responsibility, el mismo patrón que la
    /// cadena de medios del ADR-13).
    ///
    /// Si todos fallan, el Lobo responde igual —con su frase de "sin señal"—, nunca un
    /// error crudo. Así se cumple la meta de la fase: con la IA caída, la app no se rompe
    /// y el personaje sigue en pie (05-VISION-PRODUCTO).
    /// </summary>
    public class CoachResiliente : ICoachIA
    {
        private readonly IReadOnlyList<IProveedorIA> _proveedores;
        private readonly ILogger<CoachResiliente> _log;

        public CoachResiliente(IEnumerable<IProveedorIA> proveedores, ILogger<CoachResiliente> log)
        {
            _proveedores = (proveedores ?? throw new ArgumentNullException(nameof(proveedores))).ToList();
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<RespuestaCoach> ConsultarAsync(
            string mensaje, string contextoPerfil, CancellationToken cancellationToken = default)
        {
            var prompt = PersonalidadLoboCoach.ConstruirPrompt(mensaje, contextoPerfil);
            var consulta = new ConsultaIA(prompt, mensaje, contextoPerfil);

            foreach (var proveedor in _proveedores)
            {
                try
                {
                    var texto = await proveedor.GenerarAsync(consulta, cancellationToken);

                    // Un proveedor que dice "ok" pero devuelve vacío no sirve: se trata
                    // como fallo para que la cadena siga, en vez de mostrar un globo vacío.
                    if (string.IsNullOrWhiteSpace(texto))
                        throw new CoachIAException($"El proveedor {proveedor.Nombre} devolvió una respuesta vacía.");

                    return new RespuestaCoach(texto.Trim(), proveedor.Nombre, proveedor.EsRespaldo);
                }
                catch (CoachIAException ex)
                {
                    // Cada fallo queda registrado —antes no se registraba nada (D-09)—,
                    // pero no corta la cadena: todavía puede contestar otro proveedor.
                    _log.LogWarning(ex, "El proveedor de IA {Proveedor} falló; se intenta el siguiente.", proveedor.Nombre);
                }
            }

            // Ni el respaldo pudo: el Lobo responde de todos modos, en su voz.
            _log.LogError("Ningún proveedor de IA pudo responder la consulta del coach.");
            return new RespuestaCoach(PersonalidadLoboCoach.RespuestaSinSenal, "Degradado", EsDegradada: true);
        }
    }
}
