using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Domain.Models
{
public class UsuarioPerfil
{
    public int Id { get; set; }
    public string? IdentityUserId { get; set; }   // <-- NUEVA: a que cuenta de Identity pertenece este perfil

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string? Nombre { get; set; }

    [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg.")]
    public double PesoKg { get; set; }

    [Range(100, 250, ErrorMessage = "La estatura debe estar entre 100 y 250 cm.")]
    public double EstaturaCm { get; set; }

    [Range(13, 100, ErrorMessage = "La edad debe estar entre 13 y 100 años.")]
    public int Edad { get; set; }

    public ObjetivoFitness? ObjetivoActual { get; set; }
    public List<RegistroProgreso> HistorialProgreso { get; set; } = new();
}

}
