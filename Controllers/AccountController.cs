using FitnessCoach.Infrastructure.Identity;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FitnessCoach.Controllers
{
    // Registro / Login / Logout. Vistas propias 
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IdentityOptions _opcionesIdentity;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IOptions<IdentityOptions> opcionesIdentity)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _opcionesIdentity = opcionesIdentity.Value;
        }

        private int MinutosDeBloqueo => (int)_opcionesIdentity.Lockout.DefaultLockoutTimeSpan.TotalMinutes;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Perfil");
            }

            // Mensaje generico: no revelar de forma explotable si el correo ya existe (estandar 1.4)
            ModelState.AddModelError(string.Empty, "No se pudo completar el registro. Revisa los datos e intenta de nuevo.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(bool demasiadosIntentos = false)
        {
            // Llega asi desde el rate limiter (ver Program.cs) cuando una IP se pasa de envios.
            if (demasiadosIntentos)
                ModelState.AddModelError(string.Empty,
                    "Demasiados intentos desde esta conexión. Esperá un minuto antes de volver a intentar.");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // lockoutOnFailure: true — cada fallo cuenta para el bloqueo configurado en Program.cs.
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Perfil");
            }

            // Se avisa del bloqueo de forma explicita: es una excepcion deliberada al mensaje
            // generico del estandar 1.4 (confirma que la cuenta existe) a cambio de que la
            // persona entienda por que no puede entrar. Ver ADR-11.
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    $"Tu cuenta quedó bloqueada temporalmente por varios intentos fallidos. " +
                    $"Volvé a intentar en {MinutosDeBloqueo} minutos.");
                return View(model);
            }

            // Para el resto de los fallos el mensaje sigue siendo identico y no distingue
            // si el correo existe o si la contrasena es incorrecta (estandar 1.4).
            ModelState.AddModelError(string.Empty,
                $"Correo o contraseña incorrectos. Tras {_opcionesIdentity.Lockout.MaxFailedAccessAttempts} " +
                $"intentos fallidos la cuenta se bloquea {MinutosDeBloqueo} minutos.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
