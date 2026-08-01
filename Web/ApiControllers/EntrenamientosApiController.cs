using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FitnessCoach.Web.ApiControllers
{
    /// <summary>
    /// Entrenamientos completados y rachas del usuario autenticado (D-26).
    ///
    /// Todo pasa por <see cref="IServicioEntrenamientos"/>: ahí viven el aislamiento por
    /// cuenta, la validación del día de rutina y el conteo de rachas por zona horaria.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/perfil/entrenamientos")]
    [Produces("application/json")]
    public class EntrenamientosApiController : ControllerBase
    {
        private readonly IServicioEntrenamientos _entrenamientos;

        public EntrenamientosApiController(IServicioEntrenamientos entrenamientos)
        {
            _entrenamientos = entrenamientos;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Obtiene los entrenamientos completados del usuario autenticado.
        /// </summary>
        /// <returns>Entrenamientos del más reciente al más antiguo</returns>
        /// <response code="200">Historial devuelto exitosamente</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<EntrenamientoCompletado>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerHistorial() => Ok(_entrenamientos.ObtenerHistorial(IdentityId));

        /// <summary>
        /// Obtiene la racha actual y la más larga del usuario autenticado. Los días se
        /// cuentan en su zona horaria, no en la del servidor.
        /// </summary>
        /// <response code="200">Rachas calculadas</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet("rachas")]
        [ProducesResponseType(typeof(Rachas), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerRachas() => Ok(_entrenamientos.ObtenerRachas(IdentityId));

        /// <summary>
        /// Obtiene los días de la rutina del usuario, que son las únicas etiquetas que
        /// acepta el registro de un entrenamiento. Vacío si todavía no definió su objetivo.
        /// </summary>
        /// <response code="200">Días válidos de la rutina</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet("opciones")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerOpciones() => Ok(_entrenamientos.OpcionesDeRutina(IdentityId));

        /// <summary>
        /// Registra un entrenamiento completado. El nombre tiene que ser uno de los días de
        /// la rutina del usuario (ver <c>GET opciones</c>).
        /// </summary>
        /// <param name="entrenamiento">Día de la rutina, duración y notas</param>
        /// <returns>El entrenamiento registrado</returns>
        /// <response code="201">Entrenamiento registrado</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="422">El nombre no es un día de tu rutina</response>
        [HttpPost]
        [ProducesResponseType(typeof(EntrenamientoCompletado), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Registrar([FromBody] NuevoEntrenamientoRequest entrenamiento)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var registrado = _entrenamientos.Registrar(IdentityId,
                    entrenamiento.NombreRutina, entrenamiento.DuracionMinutos, entrenamiento.Notas);

                return CreatedAtAction(nameof(ObtenerHistorial), null, registrado);
            }
            catch (ArgumentException ex)
            {
                // Bien formado (eso ya lo cubre ModelState) pero no es un día de su rutina.
                return UnprocessableEntity(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un entrenamiento del usuario autenticado.
        /// </summary>
        /// <param name="id">Id del entrenamiento</param>
        /// <response code="204">Entrenamiento eliminado</response>
        /// <response code="401">No hay sesión iniciada</response>
        /// <response code="404">No existe ese entrenamiento en tu historial</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Eliminar(int id) =>
            // Un id ajeno no aparece en el historial propio: 404, no 403 (estándar §1.4).
            _entrenamientos.Eliminar(IdentityId, id) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Lo único que el cliente puede mandar al registrar un entrenamiento: la fecha y el
    /// dueño los pone el servidor.
    /// </summary>
    public class NuevoEntrenamientoRequest
    {
        [Required(ErrorMessage = "Elige un día de tu rutina.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        public string NombreRutina { get; set; } = string.Empty;

        [Range(RangosPerfil.DuracionMinimaMin, RangosPerfil.DuracionMaximaMin,
            ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        public int DuracionMinutos { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string? Notas { get; set; }
    }
}
