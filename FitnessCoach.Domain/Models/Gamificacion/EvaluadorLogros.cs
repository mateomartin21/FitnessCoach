namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>Un logro visto contra los hechos de un usuario: si está desbloqueado y cuánto lleva.</summary>
    public sealed record LogroEvaluado(Logro Logro, bool Desbloqueado, int ProgresoActual)
    {
        /// <summary>Avance de 0 a 100 hacia el logro, para la barra de progreso.</summary>
        public int Porcentaje =>
            Logro.Objetivo <= 0 ? 100 : (int)(100.0 * ProgresoActual / Logro.Objetivo);
    }

    /// <summary>
    /// Evalúa el catálogo de logros contra los hechos del usuario. Función pura: no
    /// guarda qué estaba desbloqueado antes, lo deriva del estado actual. Detectar un
    /// logro "recién conseguido" es comparar dos evaluaciones (antes/después de una acción).
    /// </summary>
    public static class EvaluadorLogros
    {
        public static IReadOnlyList<LogroEvaluado> Evaluar(EstadisticasUsuario e) =>
            CatalogoLogros.Todos
                .Select(l => new LogroEvaluado(l, l.EstaDesbloqueado(e), l.ProgresoActual(e)))
                .ToList();

        /// <summary>XP total que aportan los logros ya desbloqueados.</summary>
        public static int XpDesbloqueado(EstadisticasUsuario e) =>
            CatalogoLogros.Todos.Where(l => l.EstaDesbloqueado(e)).Sum(l => l.Xp);

        /// <summary>
        /// Los logros que se desbloquean al pasar de un estado a otro. Se usa para
        /// avisarle al usuario "¡conseguiste esto!" justo después de registrar algo.
        /// </summary>
        public static IReadOnlyList<Logro> ReciénDesbloqueados(EstadisticasUsuario antes, EstadisticasUsuario despues) =>
            CatalogoLogros.Todos
                .Where(l => !l.EstaDesbloqueado(antes) && l.EstaDesbloqueado(despues))
                .ToList();
    }
}
