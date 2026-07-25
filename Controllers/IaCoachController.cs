using FitnessCoach.Application.Coaching;
using FitnessCoach.Application.Services;
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
        private readonly ICoachIA _coach;
        private readonly IArmadorContextoCoach _contexto;

        public IaCoachController(IServicioPerfilUsuario perfiles, ICoachIA coach, IArmadorContextoCoach contexto)
        {
            _perfiles = perfiles;
            _coach = coach;
            _contexto = contexto;
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

            // Contexto rico: el Lobo ve el plan, la rutina, el diario y los récords reales,
            // no solo cuatro datos del perfil. Sin perfil todavía, un contexto mínimo.
            var contexto = usuario is null
                ? "El usuario todavía no configuró su perfil."
                : _contexto.Construir(usuario);

            // La cadena siempre devuelve algo: si la IA falló, viene la respuesta del Lobo
            // en modo sin conexión, nunca un error. El controlador ya no distingue casos.
            var respuesta = await _coach.ConsultarAsync(request.Mensaje, contexto);
            return Ok(new { respuesta = respuesta.Texto, degradada = respuesta.EsDegradada });
        }
    }

    public class ConsultaRequest
    {
        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El mensaje debe tener entre 1 y 2000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
