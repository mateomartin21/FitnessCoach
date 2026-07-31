using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Cuenta días consecutivos con al menos un entrenamiento.
    /// Es una función pura sobre fechas: sin estado, sin base de datos, sin reloj propio
    /// (el "hoy" se pasa como parámetro para poder probarla).
    /// </summary>
    public static class CalculadorRachas
    {
        /// <summary>
        /// </summary>
        /// <param name="fechasEntrenadas">Fechas de los entrenamientos, en la zona horaria en que se quiere contar el día.</param>
        /// <param name="hoy">Día de referencia para la racha actual.</param>
        public static Rachas Calcular(IEnumerable<DateTime> fechasEntrenadas, DateOnly hoy)
        {
            // Varios entrenamientos el mismo día cuentan como un solo día de racha.
            var dias = fechasEntrenadas
                .Select(DateOnly.FromDateTime)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (dias.Count == 0) return Rachas.Vacia;

            int masLarga = 1;
            int corriendo = 1;

            for (int i = 1; i < dias.Count; i++)
            {
                bool esElDiaSiguiente = dias[i] == dias[i - 1].AddDays(1);
                corriendo = esElDiaSiguiente ? corriendo + 1 : 1;
                if (corriendo > masLarga) masLarga = corriendo;
            }

            return new Rachas(CalcularRachaActual(dias, hoy), masLarga);
        }

        private static int CalcularRachaActual(List<DateOnly> diasOrdenados, DateOnly hoy)
        {
            var ultimoDia = diasOrdenados[^1];

            // Entrenar ayer todavía cuenta: si la racha se cortara a la medianoche, alguien
            // que entrena todas las tardes vería su racha en cero cada mañana.
            if (ultimoDia != hoy && ultimoDia != hoy.AddDays(-1))
                return 0;

            int racha = 1;
            for (int i = diasOrdenados.Count - 1; i > 0; i--)
            {
                if (diasOrdenados[i - 1] != diasOrdenados[i].AddDays(-1)) break;
                racha++;
            }

            return racha;
        }
    }
}
