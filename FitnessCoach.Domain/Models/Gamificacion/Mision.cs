namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Un objetivo de corto plazo que se mide sobre la última semana y se reinicia con
    /// ella. A diferencia de un <see cref="Logro"/> (permanente, histórico), la misión
    /// empuja a mover algo esta semana. Se mide sobre los campos semanales del snapshot.
    /// </summary>
    public sealed record Mision(
        string Id,
        string Nombre,
        string Descripcion,
        string Icono,
        int Xp,
        int Objetivo,
        Func<EstadisticasUsuario, int> Medir)
    {
        public int ProgresoActual(EstadisticasUsuario e) => Math.Min(Medir(e), Objetivo);

        public bool EstaCumplida(EstadisticasUsuario e) => Medir(e) >= Objetivo;
    }

    /// <summary>Una misión vista contra la semana del usuario: si está cumplida y cuánto lleva.</summary>
    public sealed record MisionEvaluada(Mision Mision, bool Cumplida, int ProgresoActual)
    {
        public int Porcentaje =>
            Mision.Objetivo <= 0 ? 100 : (int)(100.0 * ProgresoActual / Mision.Objetivo);
    }
}
