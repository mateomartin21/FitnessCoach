using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
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
        public IActionResult RegistrarPeso(double NuevoPeso, string Notas)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            usuario.HistorialProgreso.Add(new RegistroProgreso
            {
                Fecha = DateTime.Now,
                PesoKg = NuevoPeso,
                Notas = Notas ?? ""
            });
            usuario.PesoKg = NuevoPeso;

            _perfiles.Guardar(usuario);
            return RedirectToAction("Index");
        }
    }
}
