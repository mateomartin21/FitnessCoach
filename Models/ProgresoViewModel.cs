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
