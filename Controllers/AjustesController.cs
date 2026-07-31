using FitnessCoach.Application.Services;
using FitnessCoach.Infrastructure.Identity;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    // Todo lo que se configura una vez y no se toca a diario: la cuenta, el calendario
    // en el que se cuentan los días y los atajos a las preferencias de cada sección.
    [Authorize]
    public class AjustesController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly UserManager<ApplicationUser> _usuarios;
        private readonly SignInManager<ApplicationUser> _sesiones;

        public AjustesController(IServicioPerfilUsuario perfiles,
                                 UserManager<ApplicationUser> usuarios,
                                 SignInManager<ApplicationUser> sesiones)
        {
            _perfiles = perfiles;
            _usuarios = usuarios;
            _sesiones = sesiones;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var perfil = _perfiles.ObtenerOCrear(IdentityId);

            return View(new AjustesViewModel
            {
                Correo = User.Identity?.Name ?? string.Empty,
                ZonaHoraria = perfil.ZonaHoraria,
                DietasSeguidas = perfil.Preferencias.DietasSeguidas.Count,
                AlimentosExcluidos = perfil.Preferencias.AlimentosExcluidos.Count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarZonaHoraria(string? zonaHoraria)
        {
            // Un id que el sistema no reconoce correría todas las rachas sin que se note,
            // así que si no resuelve se conserva la anterior (D-25).
            if (string.IsNullOrWhiteSpace(zonaHoraria) || !ZonaHorariaUsuario.EsValida(zonaHoraria))
            {
                TempData["AjustesError"] = "Esa zona horaria no existe. No se cambió nada.";
                return RedirectToAction(nameof(Index));
            }

            var perfil = _perfiles.ObtenerOCrear(IdentityId);
            perfil.ZonaHoraria = zonaHoraria;
            _perfiles.Guardar(perfil);

            TempData["AjustesOk"] = "Listo, tu zona horaria quedó guardada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CambiarContrasena() => View(new CambiarContrasenaViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var usuario = await _usuarios.GetUserAsync(User);
            if (usuario is null) return Challenge();

            var resultado = await _usuarios.ChangePasswordAsync(
                usuario, modelo.ContrasenaActual, modelo.ContrasenaNueva);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, TraducirError(error));

                return View(modelo);
            }

            // Cambiar la contraseña rota el sello de seguridad y la cookie actual queda
            // invalidada: sin esto el usuario se encontraría deslogueado al navegar.
            await _sesiones.RefreshSignInAsync(usuario);

            TempData["AjustesOk"] = "Tu contraseña quedó actualizada.";
            return RedirectToAction(nameof(Index));
        }

        // Identity trae los mensajes en inglés y el resto de la app habla español.
        private static string TraducirError(IdentityError error) => error.Code switch
        {
            "PasswordMismatch" => "La contraseña actual no es correcta.",
            "PasswordTooShort" => "La nueva contraseña es muy corta: necesita al menos 8 caracteres.",
            "PasswordRequiresDigit" => "La nueva contraseña necesita al menos un número.",
            "PasswordRequiresLower" => "La nueva contraseña necesita al menos una minúscula.",
            "PasswordRequiresUpper" => "La nueva contraseña necesita al menos una mayúscula.",
            _ => "No se pudo cambiar la contraseña. Revisa los datos e intenta de nuevo."
        };
    }
}
