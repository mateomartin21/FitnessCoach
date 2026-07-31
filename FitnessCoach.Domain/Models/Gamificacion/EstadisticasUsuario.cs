namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Foto de los hechos del usuario que alimenta toda la gamificación: XP, nivel,
    /// logros y misiones. Es un dato plano, sin lógica: lo arma la capa de aplicación
    /// desde el perfil (que sí sabe de rachas y fechas), y los calculadores de dominio
    /// solo lo leen. Así la lógica de juego queda pura y testeable sin base de datos.
    ///
    /// Todo se deriva de hechos ya registrados (entrenamientos, récords, peso, diario),
    /// no se guarda un estado de juego aparte que pudiera desincronizarse.
    /// </summary>
    public readonly record struct EstadisticasUsuario(
        // ---- Histórico (toda la vida del usuario en la app) ----
        int TotalEntrenamientos,
        int TotalRecords,
        int DiasConDiario,
        int TotalRegistrosPeso,
        int RachaActual,
        int RachaMaxima,
        bool TieneObjetivo,

        // ---- Esta semana (últimos 7 días), para las misiones ----
        int EntrenamientosEstaSemana,
        int RegistrosPesoEstaSemana,
        int DiasConDiarioEstaSemana)
    {
        /// <summary>Un usuario recién llegado, sin ningún hecho registrado.</summary>
        public static readonly EstadisticasUsuario Vacias = new(
            0, 0, 0, 0, 0, 0, false, 0, 0, 0);
    }
}
