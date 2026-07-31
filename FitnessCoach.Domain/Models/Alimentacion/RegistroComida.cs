namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Un alimento que el usuario registró haber comido, con su cantidad y los macros
    /// que aportó.
    ///
    /// Guarda una copia de los macros al momento de registrarlo, no una referencia viva
    /// al catálogo: "lo que comí ayer" es un hecho del pasado y no debe cambiar si mañana
    /// se corrige la ficha del alimento. Mismo criterio que <see cref="RecordPersonal"/>,
    /// que copia el nombre del ejercicio.
    /// </summary>
    public class RegistroComida
    {
        public int Id { get; set; }

        /// <summary>El día del registro, en UTC. La conversión a local se hace al mostrar.</summary>
        public DateTime Fecha { get; set; }

        public string AlimentoSlug { get; set; } = string.Empty;

        /// <summary>Copia del nombre, para mostrar el diario sin consultar el catálogo.</summary>
        public string AlimentoNombre { get; set; } = string.Empty;

        public double Gramos { get; set; }

        public double Calorias { get; set; }
        public double ProteinaG { get; set; }
        public double CarbohidratoG { get; set; }
        public double GrasaG { get; set; }

        /// <summary>
        /// Arma el registro escalando los macros del alimento a la cantidad comida.
        /// Es el único camino para crear uno: garantiza que los macros guardados
        /// correspondan de verdad a los gramos, sin sumas a mano.
        /// </summary>
        public static RegistroComida De(Alimento alimento, double gramos, DateTime fecha)
        {
            ArgumentNullException.ThrowIfNull(alimento);

            var macros = alimento.MacrosPara(gramos);   // valida gramos >= 0

            return new RegistroComida
            {
                Fecha = fecha,
                AlimentoSlug = alimento.Slug,
                AlimentoNombre = alimento.Nombre,
                Gramos = gramos,
                Calorias = macros.Calorias,
                ProteinaG = macros.ProteinaG,
                CarbohidratoG = macros.CarbohidratoG,
                GrasaG = macros.GrasaG
            };
        }
    }
}
