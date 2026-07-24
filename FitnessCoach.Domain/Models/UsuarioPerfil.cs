using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Domain.Models
{
public class UsuarioPerfil
{
    public int Id { get; set; }
    public string? IdentityUserId { get; set; }   // <-- NUEVA: a que cuenta de Identity pertenece este perfil

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(RangosPerfil.NombreLargoMaximo, MinimumLength = RangosPerfil.NombreLargoMinimo,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string? Nombre { get; set; }

    [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
        ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
    public double PesoKg { get; set; }

    [Range(RangosPerfil.EstaturaMinimaCm, RangosPerfil.EstaturaMaximaCm,
        ErrorMessage = "La estatura debe estar entre {1} y {2} cm.")]
    public double EstaturaCm { get; set; }

    [Range(RangosPerfil.EdadMinima, RangosPerfil.EdadMaxima,
        ErrorMessage = "La edad debe estar entre {1} y {2} años.")]
    public int Edad { get; set; }

    public ObjetivoFitness? ObjetivoActual { get; set; }
    public List<RegistroProgreso> HistorialProgreso { get; set; } = new();
    public List<EntrenamientoCompletado> EntrenamientosCompletados { get; set; } = new();
}

}
