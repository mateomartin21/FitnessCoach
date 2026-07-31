namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// La definición de un logro desbloqueable. El criterio se expresa como una medida
    /// sobre los hechos del usuario (<see cref="Medir"/>) contra un objetivo: así el
    /// mismo modelo sirve para logros de sí/no (objetivo 1) y para los de progreso
    /// ("10 entrenamientos"), que pueden dibujar una barra.
    ///
    /// Cada logro trae la línea con que el Lobo lo festeja, porque la personalidad es
    /// del producto (05-VISION-PRODUCTO): el logro y su reacción viajan juntos.
    /// </summary>
    public sealed record Logro(
        string Id,
        string Nombre,
        string Descripcion,
        string Icono,
        int Xp,
        string LineaLobo,
        int Objetivo,
        Func<EstadisticasUsuario, int> Medir)
    {
        /// <summary>Cuánto lleva el usuario hacia este logro, tope en el objetivo.</summary>
        public int ProgresoActual(EstadisticasUsuario e) => Math.Min(Medir(e), Objetivo);

        public bool EstaDesbloqueado(EstadisticasUsuario e) => Medir(e) >= Objetivo;
    }
}
