using FitnessCoach.Application.Services;
using FitnessCoach.Infrastructure.Adapters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class IaCoachController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly GeminiCoachService _gemini;

        public IaCoachController(IServicioPerfilUsuario perfiles, GeminiCoachService gemini)
        {
            _perfiles = perfiles;
            _gemini = gemini;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]   // el token llega por la cabecera RequestVerificationToken (ver Program.cs)
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            // Sin [ApiController] el ModelState no se verifica solo: hay que mirarlo a mano.
            if (!ModelState.IsValid)
                return BadRequest(new { respuesta = "El mensaje no es válido. Escribí entre 1 y 2000 caracteres." });

            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var perfil = usuario == null ? "Usuario sin perfil configurado." :
                $"Nombre: {usuario.Nombre}, Edad: {usuario.Edad} anos, Peso: {usuario.PesoKg}kg, Estatura: {usuario.EstaturaCm}cm, Objetivo: {usuario.ObjetivoActual?.Nombre ?? "No definido"}";

            var respuesta = await _gemini.ConsultarAsync(request.Mensaje, perfil);
            return Ok(new { respuesta });
        }
    }

    public class ConsultaRequest
    {
        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El mensaje debe tener entre 1 y 2000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
