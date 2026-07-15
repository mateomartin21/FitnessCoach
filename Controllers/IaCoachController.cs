using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Adapters;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class IaCoachController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly GeminiCoachService _gemini;

        public IaCoachController(IRepositorioUsuario repositorio, GeminiCoachService gemini)
        {
            _repositorio = repositorio;
            _gemini = gemini;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            var usuario = _repositorio.ObtenerPorId(1);
            var perfil = usuario == null ? "Usuario sin perfil configurado." :
                $"Nombre: {usuario.Nombre}, Edad: {usuario.Edad} anos, Peso: {usuario.PesoKg}kg, Estatura: {usuario.EstaturaCm}cm, Objetivo: {usuario.ObjetivoActual?.Nombre ?? "No definido"}";

            var respuesta = await _gemini.ConsultarAsync(request.Mensaje, perfil);
            return Ok(new { respuesta });
        }
    }

    public class ConsultaRequest
    {
        public string Mensaje { get; set; } = string.Empty;
    }
}
