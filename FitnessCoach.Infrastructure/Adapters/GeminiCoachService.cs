using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FitnessCoach.Infrastructure.Adapters
{
    public class GeminiCoachService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiCoachService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key no configurada");
            _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
        }

        public async Task<string> ConsultarAsync(string mensaje, string perfilUsuario)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var prompt = $@"Eres el Lobo Coach, un entrenador personal experto, motivador y directo.
Tienes acceso al perfil del usuario:
{perfilUsuario}

Responde siempre en espanol, de forma concisa (maximo 3 parrafos), practica y motivadora.
No uses markdown con asteriscos. Usa lenguaje natural y cercano.

Pregunta del usuario: {mensaje}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(responseJson);

                // Verificar si hay error en la respuesta
                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    var errorMsg = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Error desconocido";
                    return $"Error de conexion con el coach: {errorMsg}";
                }

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                    return "El coach no pudo generar una respuesta. Intenta de nuevo.";

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No pude generar una respuesta. Intenta de nuevo.";
            }
            catch (Exception)
            {
                return "Hubo un problema al procesar la respuesta del coach. Intenta de nuevo.";
            }
        }
    }
}
