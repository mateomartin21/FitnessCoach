namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Las misiones de la semana y su evaluación. Son pocas y accionables a propósito:
    /// una lista larga de misiones abruma en vez de enganchar. Todas se miden sobre los
    /// hechos de los últimos 7 días del snapshot, así que se "reinician" solas al pasar
    /// la ventana, sin guardar estado.
    /// </summary>
    public static class CalculadorMisiones
    {
        public static readonly IReadOnlyList<Mision> DeLaSemana = new[]
        {
            new Mision("semana-entrenar", "Constancia semanal",
                "Entrena 3 veces esta semana.", "🔥", 50,
                3, e => e.EntrenamientosEstaSemana),

            new Mision("semana-pesarse", "Control de peso",
                "Registra tu peso al menos una vez esta semana.", "⚖️", 20,
                1, e => e.RegistrosPesoEstaSemana),

            new Mision("semana-diario", "Come con cabeza",
                "Anota tu comida en 4 días esta semana.", "🍽️", 40,
                4, e => e.DiasConDiarioEstaSemana),
        };

        public static IReadOnlyList<MisionEvaluada> Evaluar(EstadisticasUsuario e) =>
            DeLaSemana
                .Select(m => new MisionEvaluada(m, m.EstaCumplida(e), m.ProgresoActual(e)))
                .ToList();

        /// <summary>XP que aportan las misiones cumplidas de la semana.</summary>
        public static int XpCumplido(EstadisticasUsuario e) =>
            DeLaSemana.Where(m => m.EstaCumplida(e)).Sum(m => m.Xp);
    }
}
