namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// El nivel del usuario y su avance dentro de él. Pensado para dibujarse como una
    /// barra de XP de RPG (05-VISION-PRODUCTO): cuánto lleva en este nivel y cuánto
    /// falta para el próximo.
    /// </summary>
    public readonly record struct Nivel(
        int Numero,
        string Titulo,
        int XpTotal,
        int XpEnNivel,
        int XpParaSubir)
    {
        /// <summary>Avance dentro del nivel actual, de 0 a 100, para la barra.</summary>
        public int PorcentajeAvance =>
            XpParaSubir <= 0 ? 100 : (int)(100.0 * XpEnNivel / XpParaSubir);

        /// <summary>Cuánto XP falta para el siguiente nivel.</summary>
        public int XpRestante => System.Math.Max(0, XpParaSubir - XpEnNivel);
    }
}
