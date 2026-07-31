using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que muestra el centro de ajustes. La cuenta y los conteos son de solo lectura:
    /// el único campo que este formulario envía es la zona horaria.
    /// </summary>
    public class AjustesViewModel
    {
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Zona horaria")]
        public string? ZonaHoraria { get; set; }

        public int DietasSeguidas { get; set; }
        public int AlimentosExcluidos { get; set; }

        /// <summary>Grupos de equipo marcados. Vacío = sin filtrar la rutina.</summary>
        public List<string> EquipoDisponible { get; set; } = new();
    }
}
