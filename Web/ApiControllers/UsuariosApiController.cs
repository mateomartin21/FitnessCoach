using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Web.ApiControllers
{
    /// <summary>
    /// Endpoints del perfil del usuario autenticado.
    /// No recibe ningún id: el dueño sale siempre de la identidad de la petición.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/perfil")]
    [Produces("application/json")]
    public class UsuariosApiController : ControllerBase
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly ICalculadorCalorico _calculador;

        public UsuariosApiController(IServicioPerfilUsuario perfiles, ICalculadorCalorico calculador)
        {
            _perfiles = perfiles;
            _calculador = calculador;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Obtiene el perfil del usuario autenticado.
        /// </summary>
        /// <returns>Perfil del usuario de la sesión actual</returns>
        /// <response code="200">Perfil encontrado</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet]
        [ProducesResponseType(typeof(PerfilResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerMiPerfil()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            return Ok(PerfilResponse.Desde(usuario));
        }

        /// <summary>
        /// Calcula las calorías diarias recomendadas para el usuario autenticado.
        /// </summary>
        /// <returns>Calorías diarias recomendadas</returns>
        /// <response code="200">Cálculo exitoso</response>
        /// <response code="401">No hay sesión iniciada</response>
        [HttpGet("calorias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ObtenerCalorias()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            try
            {
                var calorias = _calculador.CalcularCaloriasDiarias(usuario);
                return Ok(new { caloriasRecomendadas = Math.Round(calorias, 0) });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // El perfil tiene datos fuera de rango: no es un error del servidor,
                // es un perfil que hay que completar antes de poder calcular.
                return UnprocessableEntity(new { mensaje = ex.Message });
            }
        }
    }

    /// <summary>
    /// Vista pública del perfil: deja fuera el IdentityUserId, que es interno.
    /// </summary>
    public record PerfilResponse(
        string? Nombre,
        int Edad,
        double PesoKg,
        double EstaturaCm,
        string? Objetivo)
    {
        public static PerfilResponse Desde(UsuarioPerfil usuario) => new(
            usuario.Nombre,
            usuario.Edad,
            usuario.PesoKg,
            usuario.EstaturaCm,
            usuario.ObjetivoActual?.Nombre);
    }
}
