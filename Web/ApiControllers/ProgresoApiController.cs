using FitnessCoach.Models;
using FitnessCoach.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Web.ApiControllers
{
    /// <summary>
    /// Endpoints para registro y consulta del historial de progreso de peso.
    /// </summary>
    [ApiController]
    [Route("api/usuarios/{usuarioId}/progreso")]
    [Produces("application/json")]
    public class ProgresoApiController : ControllerBase
    {
        private readonly IRepositorioUsuario _repositorio;

        public ProgresoApiController(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        /// <summary>
        /// Obtiene el historial de progreso completo de un usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Lista de registros de progreso ordenados por fecha</returns>
        /// <response code="200">Historial devuelto exitosamente</response>
        /// <response code="404">No existe un usuario con ese ID</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RegistroProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerHistorial(int usuarioId)
        {
            var usuario = _repositorio.ObtenerPorId(usuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = $"No se encontró un usuario con ID {usuarioId}." });

            var historial = usuario.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .ToList();

            return Ok(historial);
        }

        /// <summary>
        /// Obtiene el registro de progreso más reciente de un usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Registro de progreso más reciente</returns>
        /// <response code="200">Registro encontrado</response>
        /// <response code="404">Usuario no encontrado o sin historial</response>
        [HttpGet("ultimo")]
        [ProducesResponseType(typeof(RegistroProgreso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerUltimo(int usuarioId)
        {
            var usuario = _repositorio.ObtenerPorId(usuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = $"No se encontró un usuario con ID {usuarioId}." });

            var ultimo = usuario.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();

            if (ultimo == null)
                return NotFound(new { mensaje = "El usuario aún no tiene registros de progreso." });

            return Ok(ultimo);
        }

        /// <summary>
        /// Agrega un nuevo registro de progreso para un usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="registro">Datos del registro (peso y notas)</param>
        /// <returns>El registro creado</returns>
        /// <response code="201">Registro agregado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">No existe un usuario con ese ID</response>
        [HttpPost]
        [ProducesResponseType(typeof(RegistroProgreso), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AgregarRegistro(int usuarioId, [FromBody] RegistroProgreso registro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _repositorio.ObtenerPorId(usuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = $"No se encontró un usuario con ID {usuarioId}." });

            registro.Fecha = DateTime.UtcNow;
            usuario.HistorialProgreso.Add(registro);
            _repositorio.Guardar(usuario);

            return CreatedAtAction(nameof(ObtenerUltimo), new { usuarioId }, registro);
        }
    }
}
