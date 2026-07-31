using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// Arma el contexto que el Lobo recibe antes de responder: todo lo que el sistema
    /// ya sabe del usuario (perfil, plan de comidas, rutina, diario, récords) más la
    /// lista real de alimentos del catálogo.
    ///
    /// Con esto la IA deja de responder a ciegas: ve la situación completa y sus
    /// recomendaciones se anclan a lo que de verdad existe en la app, sin inventar.
    /// </summary>
    public interface IArmadorContextoCoach
    {
        string Construir(UsuarioPerfil usuario);
    }
}
