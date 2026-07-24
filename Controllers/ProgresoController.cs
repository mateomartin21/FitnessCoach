using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class ProgresoController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ProgresoController(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            return View(ArmarVista(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarPeso([Bind(Prefix = "Nuevo")] RegistrarPesoViewModel modelo)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            // Nada se guarda hasta saber que los datos son válidos.
            if (!ModelState.IsValid)
                return View("Index", ArmarVista(usuario, modelo));

            usuario.HistorialProgreso.Add(new RegistroProgreso
            {
                Fecha = DateTime.UtcNow,   // siempre UTC; la vista convierte al mostrar (D-10)
                PesoKg = modelo.NuevoPeso,
                Notas = modelo.Notas ?? ""
            });
            usuario.PesoKg = modelo.NuevoPeso;

            _perfiles.Guardar(usuario);
            TempData["MensajeProgreso"] = "Registro guardado.";
            return RedirectToAction("Index");
        }

        private static ProgresoViewModel ArmarVista(UsuarioPerfil usuario, RegistrarPesoViewModel? nuevo = null) => new()
        {
            Nuevo = nuevo ?? new RegistrarPesoViewModel { NuevoPeso = usuario.PesoKg },
            Historial = usuario.HistorialProgreso.OrderByDescending(r => r.Fecha).ToList(),
            PesoActual = usuario.PesoKg
        };
    }
}
