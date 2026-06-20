using FitnessCoach.Models;
using FitnessCoach.Repositories;
using FitnessCoach.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Web.ApiControllers
{
    /// <summary>
    /// Endpoints para gestión de perfiles de usuario.
    /// </summary>
    [ApiController]
    [Route("api/usuarios")]
    [Produces("application/json")]
    public class UsuariosApiController : ControllerBase
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly ICalculadorCalorico _calculador;

        public UsuariosApiController(IRepositorioUsuario repositorio, ICalculadorCalorico calculador)
        {
            _repositorio = repositorio;
            _calculador = calculador;
        }

        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Perfil completo del usuario</returns>
        /// <response code="200">Perfil encontrado</response>
        /// <response code="404">No existe un usuario con ese ID</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioPerfil), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerPorId(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"No se encontró un usuario con ID {id}." });

            return Ok(usuario);
        }

        /// <summary>
        /// Crea un nuevo perfil de usuario.
        /// </summary>
        /// <param name="usuario">Datos del nuevo usuario</param>
        /// <returns>Usuario creado con su ID asignado</returns>
        /// <response code="201">Usuario creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioPerfil), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Crear([FromBody] UsuarioPerfil usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            usuario.Id = 0; // El repositorio asigna el ID
            _repositorio.Guardar(usuario);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
        }

        /// <summary>
        /// Calcula las calorías diarias recomendadas para un usuario.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Calorías diarias recomendadas</returns>
        /// <response code="200">Cálculo exitoso</response>
        /// <response code="404">No existe un usuario con ese ID</response>
        [HttpGet("{id}/calorias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerCalorias(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"No se encontró un usuario con ID {id}." });

            var calorias = _calculador.CalcularCaloriasDiarias(usuario);
            return Ok(new { usuarioId = id, caloriasRecomendadas = calorias });
        }
    }
}
