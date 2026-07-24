using FitnessCoach.Application.Services;
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
        private readonly IServicioProgreso _progreso;
        private readonly IServicioEntrenamientos _entrenamientos;

        public ProgresoController(IServicioPerfilUsuario perfiles,
                                  IServicioProgreso progreso,
                                  IServicioEntrenamientos entrenamientos)
        {
            _perfiles = perfiles;
            _progreso = progreso;
            _entrenamientos = entrenamientos;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            return View(ArmarVista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarPeso([Bind(Prefix = "Nuevo")] RegistrarPesoViewModel modelo)
        {
            // Nada se guarda hasta saber que los datos son válidos.
            if (!ModelState.IsValid)
                return View("Index", ArmarVista(modelo));

            _progreso.Agregar(IdentityId, modelo.NuevoPeso, modelo.Notas);

            TempData["MensajeProgreso"] = "Registro guardado.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            // El servicio busca dentro del historial del usuario autenticado. Un id ajeno
            // sencillamente no aparece: 404, no 403, para no confirmar que existe (estándar §1.4).
            var registro = _progreso.ObtenerRegistro(IdentityId, id);
            if (registro is null) return NotFound();

            return View(new EditarRegistroViewModel
            {
                Id = registro.Id,
                Fecha = registro.Fecha,
                PesoKg = registro.PesoKg,
                Notas = registro.Notas
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(EditarRegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            if (!_progreso.Editar(IdentityId, modelo.Id, modelo.PesoKg, modelo.Notas))
                return NotFound();

            TempData["MensajeProgreso"] = "Registro actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!_progreso.Eliminar(IdentityId, id))
                return NotFound();

            TempData["MensajeProgreso"] = "Registro eliminado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarEntrenamiento(
            [Bind(Prefix = "NuevoEntrenamiento")] RegistrarEntrenamientoViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", ArmarVista(nuevoEntrenamiento: modelo));

            _entrenamientos.Registrar(IdentityId, modelo.NombreRutina, modelo.DuracionMinutos, modelo.Notas);

            TempData["MensajeProgreso"] = "Entrenamiento registrado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarEntrenamiento(int id)
        {
            if (!_entrenamientos.Eliminar(IdentityId, id))
                return NotFound();

            TempData["MensajeProgreso"] = "Entrenamiento eliminado.";
            return RedirectToAction("Index");
        }

        private ProgresoViewModel ArmarVista(
            RegistrarPesoViewModel? nuevo = null,
            RegistrarEntrenamientoViewModel? nuevoEntrenamiento = null)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            return new ProgresoViewModel
            {
                Nuevo = nuevo ?? new RegistrarPesoViewModel { NuevoPeso = usuario.PesoKg },
                Historial = _progreso.ObtenerHistorial(IdentityId).ToList(),
                PesoActual = usuario.PesoKg,
                NuevoEntrenamiento = nuevoEntrenamiento ?? new RegistrarEntrenamientoViewModel(),
                Entrenamientos = _entrenamientos.ObtenerHistorial(IdentityId).ToList(),
                Rachas = _entrenamientos.ObtenerRachas(IdentityId)
            };
        }
    }
}
