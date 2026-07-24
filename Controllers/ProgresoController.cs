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
            var historial = usuario.HistorialProgreso.OrderByDescending(r => r.Fecha).ToList();
            ViewBag.PesoActual = usuario.PesoKg;
            return View(historial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarPeso(RegistrarPesoViewModel modelo)
        {
            // Nada se guarda hasta saber que los datos son válidos.
            if (!ModelState.IsValid)
            {
                TempData["ErrorProgreso"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();
                return RedirectToAction("Index");
            }

            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            usuario.HistorialProgreso.Add(new RegistroProgreso
            {
                Fecha = DateTime.Now,
                PesoKg = modelo.NuevoPeso,
                Notas = modelo.Notas ?? ""
            });
            usuario.PesoKg = modelo.NuevoPeso;

            _perfiles.Guardar(usuario);
            return RedirectToAction("Index");
        }
    }
}
