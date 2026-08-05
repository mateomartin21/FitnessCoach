using FitnessCoach.Application.Coaching;
using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Coaching;
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
        private readonly IServicioConversacion _conversacion;

        public IaCoachController(
            IServicioPerfilUsuario perfiles,
            ICoachIA coach,
            IArmadorContextoCoach contexto,
            IServicioConversacion conversacion)
        {
            _perfiles = perfiles;
            _coach = coach;
            _contexto = contexto;
            _conversacion = conversacion;
        }

        public IActionResult Index()
        {
            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Sin perfil todavia no hay conversacion que mostrar; el chat igual funciona.
            var historial = usuario is null
                ? Array.Empty<MensajeCoach>()
                : _conversacion.Historial(usuario.Id).ToArray();

            return View(historial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]   // el token llega por la cabecera RequestVerificationToken (ver Program.cs)
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            // Sin [ApiController] el ModelState no se verifica solo: hay que mirarlo a mano.
            if (!ModelState.IsValid)
                return BadRequest(new { respuesta = "El mensaje no es válido. Escribe entre 1 y 2000 caracteres." });

            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Contexto rico: el Lobo ve el plan, la rutina, el diario y los récords reales,
            // no solo cuatro datos del perfil. Sin perfil todavía, un contexto mínimo.
            var contexto = usuario is null
                ? "El usuario todavía no configuró su perfil."
                : _contexto.Construir(usuario);

            // Los ultimos turnos van en el prompt: sin esto Koda contesta cada mensaje
            // como si fuera el primero, aunque la charla siga en pantalla.
            var memoria = usuario is null ? null : _conversacion.Memoria(usuario.Id);

            // La cadena siempre devuelve algo: si la IA falló, viene la respuesta del Lobo
            // en modo sin conexión, nunca un error. El controlador ya no distingue casos.
            var respuesta = await _coach.ConsultarAsync(request.Mensaje, contexto, memoria);

            // Se guarda incluso la respuesta degradada: al recargar, la charla tiene que
            // verse igual que antes de recargar, sin huecos.
            if (usuario is not null)
                _conversacion.RegistrarIntercambio(usuario.Id, request.Mensaje, respuesta.Texto);

            return Ok(new { respuesta = respuesta.Texto, degradada = respuesta.EsDegradada });
        }

        /// <summary>Empezar la charla de cero. La conversación es del usuario y puede borrarla.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Borrar()
        {
            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuario is not null)
                _conversacion.Borrar(usuario.Id);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// El Lobo analiza un aspecto del usuario (progreso, dieta o rutina) usando sus
        /// datos reales. Es la IA como capa sobre el sistema, no un chat: se dispara
        /// desde las pantallas de progreso/dieta/rutina.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analizar([FromBody] AnalisisRequest request)
        {
            var usuario = _perfiles.Obtener(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuario is null || usuario.ObjetivoActual is null)
                return Ok(new { respuesta = "Configura tu perfil y tu objetivo primero, campeón, y te hago el análisis.", degradada = true });

            var contexto = _contexto.Construir(usuario);
            var pedido = PersonalidadKoda.PedidoDeAnalisis(request?.Aspecto ?? "progreso");

            var respuesta = await _coach.ConsultarAsync(pedido, contexto);
            return Ok(new { respuesta = respuesta.Texto, degradada = respuesta.EsDegradada });
        }
    }

    public class ConsultaRequest
    {
        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El mensaje debe tener entre 1 y 2000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }

    public class AnalisisRequest
    {
        /// <summary>Qué mirar: "progreso", "dieta" o "rutina". Cualquier otro cae en progreso.</summary>
        public string Aspecto { get; set; } = "progreso";
    }
}
