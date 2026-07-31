using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    /// <summary>
    /// La pantalla de juego: nivel, logros y misiones de la semana. Todo se deriva de
    /// los hechos del usuario, así que no hay nada que escribir acá — solo mostrar.
    /// </summary>
    [Authorize]
    public class GamificacionController : Controller
    {
        private readonly IServicioGamificacion _gamificacion;

        public GamificacionController(IServicioGamificacion gamificacion)
        {
            _gamificacion = gamificacion;
        }

        public IActionResult Index()
        {
            var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return View(_gamificacion.Obtener(identityId));
        }
    }
}
