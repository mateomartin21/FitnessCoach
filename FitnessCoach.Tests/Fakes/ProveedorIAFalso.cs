using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// Proveedor de IA de mentira, para probar la cadena sin salir a la red.
    /// Se configura para contestar un texto, fallar, o devolver vacío.
    /// </summary>
    public class ProveedorIAFalso : IProveedorIA
    {
        private readonly string? _respuesta;
        private readonly bool _falla;

        public string Nombre { get; }
        public bool EsRespaldo { get; }

        /// <summary>Cuántas veces se lo llamó, para afirmar que la cadena no siguió de más.</summary>
        public int VecesLlamado { get; private set; }

        private ProveedorIAFalso(string nombre, string? respuesta, bool falla, bool esRespaldo)
        {
            Nombre = nombre;
            _respuesta = respuesta;
            _falla = falla;
            EsRespaldo = esRespaldo;
        }

        public static ProveedorIAFalso QueResponde(string texto, string nombre = "Falso", bool esRespaldo = false) =>
            new(nombre, texto, falla: false, esRespaldo);

        public static ProveedorIAFalso QueFalla(string nombre = "Falso") =>
            new(nombre, respuesta: null, falla: true, esRespaldo: false);

        public static ProveedorIAFalso QueDevuelveVacio(string nombre = "Falso") =>
            new(nombre, respuesta: "   ", falla: false, esRespaldo: false);

        public Task<string> GenerarAsync(ConsultaIA consulta, CancellationToken cancellationToken = default)
        {
            VecesLlamado++;

            if (_falla)
                throw new CoachIAException($"{Nombre} falló a propósito.");

            return Task.FromResult(_respuesta!);
        }
    }
}
