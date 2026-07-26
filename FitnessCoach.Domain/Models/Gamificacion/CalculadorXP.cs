namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Traduce los hechos del usuario a puntos de experiencia. La constancia es lo que
    /// más paga (05-VISION-PRODUCTO §77: el progreso sostenido vale más que acumular
    /// insignias): por eso hay un bono por la mejor racha además del XP por cada acción.
    ///
    /// Es una función pura sobre <see cref="EstadisticasUsuario"/>. El XP de los logros
    /// se suma aparte, al armar el resumen, porque depende del catálogo de logros.
    /// </summary>
    public static class CalculadorXP
    {
        public const int PorEntrenamiento = 50;
        public const int PorRecord = 40;
        public const int PorDiaConDiario = 10;
        public const int PorRegistroPeso = 15;

        /// <summary>Bono por cada día de la mejor racha: premia la constancia sostenida.</summary>
        public const int PorDiaDeMejorRacha = 25;

        /// <summary>XP base que sale de los hechos, sin contar los logros desbloqueados.</summary>
        public static int Base(EstadisticasUsuario e) =>
            e.TotalEntrenamientos * PorEntrenamiento
            + e.TotalRecords * PorRecord
            + e.DiasConDiario * PorDiaConDiario
            + e.TotalRegistrosPeso * PorRegistroPeso
            + e.RachaMaxima * PorDiaDeMejorRacha;
    }
}
