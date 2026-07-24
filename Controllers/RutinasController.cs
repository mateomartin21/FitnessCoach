using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class RutinasController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IGeneradorRutinas _generador;

        public RutinasController(IServicioPerfilUsuario perfiles, IGeneradorRutinas generador)
        {
            _perfiles = perfiles;
            _generador = generador;
        }

        public IActionResult Index()
        {
            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuario == null || usuario.ObjetivoActual == null)
                return RedirectToAction("Index", "Perfil");

            var rutinaGenerada = _generador.GenerarRutinaParaObjetivo(usuario.ObjetivoActual);
            return View(rutinaGenerada);
        }
    }
}
