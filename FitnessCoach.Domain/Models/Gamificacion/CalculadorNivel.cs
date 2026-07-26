namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// Convierte un total de XP en un nivel. La curva es creciente, como en un RPG:
    /// subir de nivel cuesta cada vez más, así que los primeros llegan rápido (enganche
    /// temprano) y los altos premian la constancia larga.
    ///
    /// Costo del nivel n → n+1 = <see cref="XpBasePorNivel"/> + (n-1) · <see cref="XpExtraPorNivel"/>.
    /// Es una función pura: mismo XP, mismo nivel, sin estado ni reloj.
    /// </summary>
    public static class CalculadorNivel
    {
        public const int XpBasePorNivel = 100;   // costo del nivel 1 → 2
        public const int XpExtraPorNivel = 50;    // cada nivel siguiente cuesta 50 más

        /// <summary>Costo en XP de pasar del nivel <paramref name="numero"/> al siguiente.</summary>
        public static int CostoDeSubirDesde(int numero) =>
            XpBasePorNivel + (numero - 1) * XpExtraPorNivel;

        public static Nivel Calcular(int xpTotal)
        {
            if (xpTotal < 0) xpTotal = 0;

            int numero = 1;
            int restante = xpTotal;

            // Va descontando el costo de cada nivel mientras alcance para subir.
            while (restante >= CostoDeSubirDesde(numero))
            {
                restante -= CostoDeSubirDesde(numero);
                numero++;
            }

            return new Nivel(
                Numero: numero,
                Titulo: TituloPara(numero),
                XpTotal: xpTotal,
                XpEnNivel: restante,
                XpParaSubir: CostoDeSubirDesde(numero));
        }

        /// <summary>El rango del Lobo según el nivel, para que subir se sienta como progresar.</summary>
        public static string TituloPara(int numero) => numero switch
        {
            <= 2 => "Cachorro",
            <= 4 => "Lobo Joven",
            <= 6 => "Rastreador",
            <= 9 => "Cazador",
            <= 12 => "Lobo Veterano",
            <= 16 => "Líder de Manada",
            _ => "Lobo Alfa",
        };
    }
}
