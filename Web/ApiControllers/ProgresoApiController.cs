using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FitnessCoach.Web.ApiControllers
{
    /// <summary>
    /// Historial de progreso de peso del usuario autenticado.
    /// La ruta ya no lleva el id del usuario: el dueño sale de la identidad.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/perfil/progreso")]
    [Produces("application/json")]
    public class ProgresoApiController : ControllerBase
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ProgresoApiController(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Obtiene el historial de progreso completo del usuario autenticado.
        /// </summary>
        /// <returns>Lista de registros de progreso ordenados por fecha</returns>
        /// <response code="200">Historial devuelto exitosamente</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RegistroProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerHistorial()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            var historial = usuario.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .ToList();

            return Ok(historial);
        }

        /// <summary>
        /// Obtiene el registro de progreso más reciente del usuario autenticado.
        /// </summary>
        /// <returns>Registro de progreso más reciente</returns>
        /// <response code="200">Registro encontrado</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="404">Todavía no hay registros de progreso</response>
        [HttpGet("ultimo")]
        [ProducesResponseType(typeof(RegistroProgreso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerUltimo()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            var ultimo = usuario.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();

            if (ultimo == null)
                return NotFound(new { mensaje = "Todavía no tenés registros de progreso." });

            return Ok(ultimo);
        }

        /// <summary>
        /// Agrega un nuevo registro de progreso al usuario autenticado.
        /// </summary>
        /// <param name="registro">Peso y notas del registro</param>
        /// <returns>El registro creado</returns>
        /// <response code="201">Registro agregado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpPost]
        [ProducesResponseType(typeof(RegistroProgreso), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AgregarRegistro([FromBody] NuevoRegistroRequest registro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            // La fecha la pone el servidor, nunca el cliente.
            var nuevo = new RegistroProgreso
            {
                Fecha = DateTime.UtcNow,
                PesoKg = registro.PesoKg,
                Notas = registro.Notas ?? string.Empty
            };

            usuario.HistorialProgreso.Add(nuevo);
            _perfiles.Guardar(usuario);

            return CreatedAtAction(nameof(ObtenerUltimo), null, nuevo);
        }
    }

    /// <summary>
    /// Lo único que el cliente puede mandar al crear un registro.
    /// </summary>
    public class NuevoRegistroRequest
    {
        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        public double PesoKg { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string? Notas { get; set; }
    }
}
