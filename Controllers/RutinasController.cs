using FitnessCoach.Application.Services;
using FitnessCoach.Models;
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
        private readonly IServicioSustitucionEjercicios _sustituciones;

        public RutinasController(IServicioPerfilUsuario perfiles,
                                 IGeneradorRutinas generador,
                                 IServicioSustitucionEjercicios sustituciones)
        {
            _perfiles = perfiles;
            _generador = generador;
            _sustituciones = sustituciones;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var usuario = _perfiles.Obtener(IdentityId);
            if (usuario == null || usuario.ObjetivoActual == null)
                return RedirectToAction("Index", "Perfil");

            // El Id del perfil como semilla: dos usuarios con el mismo objetivo reciben
            // ejercicios distintos, y cada uno ve siempre la misma rutina.
            var rutinaGenerada = _generador.GenerarRutinaParaObjetivo(
                usuario.ObjetivoActual, usuario.Id, usuario.PreferenciasEntrenamiento);

            return View(rutinaGenerada);
        }

        [HttpGet]
        public IActionResult Cambiar(string slug, string? q)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            var enUso = _sustituciones.EnUso(usuario, slug);
            if (enUso is null) return NotFound();

            var alternativas = _sustituciones.Alternativas(usuario, slug, q);

            return View(new CambiarEjercicioViewModel
            {
                SlugReferencia = slug,
                EnUso = enUso,
                EsSustituido = usuario.PreferenciasEntrenamiento.Sustituciones.ContainsKey(slug),
                TotalAlternativas = alternativas.Count,
                Busqueda = q,
                // Un grupo grande deja más de cien: sin tope la pantalla se vuelve un scroll infinito.
                Alternativas = alternativas.Take(CambiarEjercicioViewModel.Tope).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarCambio(string slugReferencia, string slugElegido)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            if (!_sustituciones.Sustituir(usuario, slugReferencia, slugElegido))
            {
                TempData["RutinaError"] = "Ese ejercicio no sirve de reemplazo: no existe o trabaja otro músculo.";
                return RedirectToAction(nameof(Cambiar), new { slug = slugReferencia });
            }

            _perfiles.Guardar(usuario);
            TempData["RutinaOk"] = "Listo, tu rutina ya tiene el ejercicio que elegiste.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restaurar(string slugReferencia)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            _sustituciones.Restaurar(usuario, slugReferencia);
            _perfiles.Guardar(usuario);

            TempData["RutinaOk"] = "Volvió el ejercicio que había elegido Koda.";
            return RedirectToAction(nameof(Index));
        }
    }
}
