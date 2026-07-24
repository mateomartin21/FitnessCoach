using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class AlimentacionController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IGeneradorAlimentacion _generador;

        public AlimentacionController(IServicioPerfilUsuario perfiles, IGeneradorAlimentacion generador)
        {
            _perfiles = perfiles;
            _generador = generador;
        }

        public IActionResult Index()
        {
            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuario == null || usuario.ObjetivoActual == null)
                return RedirectToAction("Index", "Perfil");

            var plan = _generador.GenerarPlanParaObjetivo(usuario.ObjetivoActual);
            return View(plan);
        }
    }
}
