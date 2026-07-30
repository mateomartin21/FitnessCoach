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
    ///
    /// Todo pasa por <see cref="IServicioProgreso"/>, donde viven las reglas. Antes la API
    /// tocaba el perfil por su cuenta y no coincidía con la pantalla (D-26).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/perfil/progreso")]
    [Produces("application/json")]
    public class ProgresoApiController : ControllerBase
    {
        private readonly IServicioProgreso _progreso;

        public ProgresoApiController(IServicioProgreso progreso)
        {
            _progreso = progreso;
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
        public IActionResult ObtenerHistorial() => Ok(_progreso.ObtenerHistorial(IdentityId));

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
            // El historial ya viene del más reciente al más antiguo.
            var ultimo = _progreso.ObtenerHistorial(IdentityId).FirstOrDefault();

            if (ultimo is null)
                return NotFound(new { mensaje = "Todavía no tienes registros de progreso." });

            return Ok(ultimo);
        }

        /// <summary>
        /// Obtiene un registro concreto del historial del usuario autenticado.
        /// </summary>
        /// <param name="id">Id del registro</param>
        /// <response code="200">Registro encontrado</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="404">No existe ese registro en tu historial</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RegistroProgreso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerRegistro(int id)
        {
            // Un id ajeno no aparece en el historial propio: 404, no 403 (estándar §1.4).
            var registro = _progreso.ObtenerRegistro(IdentityId, id);

            return registro is null ? NotFound() : Ok(registro);
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
        public IActionResult AgregarRegistro([FromBody] RegistroPesoRequest registro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // La fecha la pone el servicio, siempre en UTC: nunca la manda el cliente.
            var nuevo = _progreso.Agregar(IdentityId, registro.PesoKg, registro.Notas);

            return CreatedAtAction(nameof(ObtenerRegistro), new { id = nuevo.Id }, nuevo);
        }

        /// <summary>
        /// Edita el peso y las notas de un registro del usuario autenticado. La fecha no se
        /// edita: es cuándo ocurrió el hecho, no un dato que el cliente ajuste.
        /// </summary>
        /// <param name="id">Id del registro</param>
        /// <param name="registro">Nuevo peso y notas</param>
        /// <response code="204">Registro actualizado</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="404">No existe ese registro en tu historial</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult EditarRegistro(int id, [FromBody] RegistroPesoRequest registro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _progreso.Editar(IdentityId, id, registro.PesoKg, registro.Notas)
                ? NoContent()
                : NotFound();
        }

        /// <summary>
        /// Elimina un registro del historial del usuario autenticado.
        /// </summary>
        /// <param name="id">Id del registro</param>
        /// <response code="204">Registro eliminado</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="404">No existe ese registro en tu historial</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult EliminarRegistro(int id) =>
            _progreso.Eliminar(IdentityId, id) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Lo único que el cliente puede mandar al crear o editar un registro: la fecha y el
    /// dueño los pone el servidor. Los rangos son los mismos que valida el dominio.
    /// </summary>
    public class RegistroPesoRequest
    {
        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        public double PesoKg { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string? Notas { get; set; }
    }
}
