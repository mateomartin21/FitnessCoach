namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Todo el estado de juego del usuario listo para dibujar: su nivel, sus logros y
    /// las misiones de la semana. Es un agregado de solo lectura; lo arma la capa de
    /// aplicación a partir del snapshot de hechos.
    /// </summary>
    public sealed record ResumenGamificacion(
        Nivel Nivel,
        IReadOnlyList<LogroEvaluado> Logros,
        IReadOnlyList<MisionEvaluada> Misiones)
    {
        public int LogrosDesbloqueados => Logros.Count(l => l.Desbloqueado);
        public int LogrosTotales => Logros.Count;

        /// <summary>Desbloqueados primero; entre pendientes, el más cercano a completarse arriba.</summary>
        public IEnumerable<LogroEvaluado> LogrosOrdenados =>
            Logros.OrderByDescending(l => l.Desbloqueado).ThenByDescending(l => l.Porcentaje);
    }
}
