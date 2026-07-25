using Microsoft.Extensions.Logging;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// Logger de mentira que guarda lo que se registró, para poder afirmar que los
    /// fallos de los proveedores quedan anotados (D-09 exigía que se registraran).
    /// </summary>
    public class LoggerCapturador<T> : ILogger<T>
    {
        public List<(LogLevel Nivel, string Mensaje)> Registros { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NoOp();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Registros.Add((logLevel, formatter(state, exception)));
        }

        public int Advertencias => Registros.Count(r => r.Nivel == LogLevel.Warning);
        public int Errores => Registros.Count(r => r.Nivel == LogLevel.Error);

        private sealed class NoOp : IDisposable { public void Dispose() { } }
    }
}
