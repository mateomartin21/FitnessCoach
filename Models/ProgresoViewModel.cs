using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que la pantalla de Progreso necesita: el formulario de alta y el historial ya ordenado.
    /// </summary>
    public class ProgresoViewModel
    {
        /// <summary>Formulario de registro. Se liga con prefijo "Nuevo" (ver RegistrarPeso).</summary>
        public RegistrarPesoViewModel Nuevo { get; set; } = new();

        /// <summary>Historial del más reciente al más antiguo.</summary>
        public List<RegistroProgreso> Historial { get; set; } = new();

        /// <summary>Peso del perfil, que puede no coincidir con el último registro.</summary>
        public double PesoActual { get; set; }

        /// <summary>Formulario de entrenamiento. Se liga con prefijo "NuevoEntrenamiento".</summary>
        public RegistrarEntrenamientoViewModel NuevoEntrenamiento { get; set; } = new();

        /// <summary>
        /// Los días de la rutina real del usuario, únicas opciones válidas para marcar un
        /// entrenamiento: si fuera texto libre, cualquiera anota algo inventado y se lleva
        /// el XP y los logros sin haber entrenado.
        /// </summary>
        public List<string> OpcionesRutina { get; set; } = new();

        /// <summary>Sin objetivo no hay rutina, así que todavía no se puede registrar un entrenamiento.</summary>
        public bool PuedeRegistrarEntrenamiento => OpcionesRutina.Count > 0;

        public List<EntrenamientoCompletado> Entrenamientos { get; set; } = new();

        public Rachas Rachas { get; set; } = Rachas.Vacia;

        public List<RecordPersonal> Records { get; set; } = new();

        public bool TieneEntrenamientos => Entrenamientos.Count > 0;

        public bool TieneRecords => Records.Count > 0;

        public bool TieneRegistros => Historial.Count > 0;

        /// <summary>Cuánto cambió el peso entre el primer registro y el último. Null si hay menos de dos.</summary>
        public double? VariacionTotalKg
        {
            get
            {
                if (Historial.Count < 2) return null;
                var masReciente = Historial[0].PesoKg;
                var masAntiguo = Historial[^1].PesoKg;
                return Math.Round(masReciente - masAntiguo, 1);
            }
        }

        /// <summary>Con un solo punto no hay evolución que dibujar.</summary>
        public bool PuedeGraficar => Historial.Count >= 2;

        /// <summary>Registros en orden cronológico, que es como se lee una gráfica.</summary>
        private IEnumerable<RegistroProgreso> EnOrdenCronologico => Historial.OrderBy(r => r.Fecha);

        public List<string> EtiquetasGrafica =>
            EnOrdenCronologico.Select(r => r.Fecha.ToLocalTime().ToString("dd/MM")).ToList();

        public List<double> PesosGrafica =>
            EnOrdenCronologico.Select(r => r.PesoKg).ToList();

        /// <summary>Diferencia de un registro contra el inmediatamente anterior en el tiempo.</summary>
        public double? VariacionRespectoAlAnterior(int indice)
        {
            // El historial viene del más reciente al más antiguo: el anterior en el tiempo
            // es el que está una posición más abajo en la lista.
            if (indice < 0 || indice >= Historial.Count - 1) return null;
            return Math.Round(Historial[indice].PesoKg - Historial[indice + 1].PesoKg, 1);
        }
    }
}
