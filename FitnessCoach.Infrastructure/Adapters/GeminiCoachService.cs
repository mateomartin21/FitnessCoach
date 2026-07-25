using System.Text;
using System.Text.Json;
using FitnessCoach.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace FitnessCoach.Infrastructure.Adapters
{
    /// <summary>
    /// Proveedor de IA real: le manda el prompt a la API de Gemini y devuelve el texto.
    ///
    /// Ahora solo se ocupa de "cómo hablo con Google": recibe el prompt ya armado (la
    /// personalidad del Lobo vive en Application, D-20) y ante cualquier problema
    /// **lanza** <see cref="CoachIAException"/> en vez de devolver el error como texto
    /// (D-09). Así la cadena puede distinguir un fallo y pasar al respaldo.
    /// </summary>
    public class GeminiCoachService : IProveedorIA
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly string _model;

        public GeminiCoachService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // No se lanza en el constructor: sin clave, el fallo ocurre al llamar y la
            // cadena cae al respaldo, en vez de tumbar el arranque de toda la app.
            _apiKey = config["Gemini:ApiKey"];
            _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
        }

        public string Nombre => "Gemini";
        public bool EsRespaldo => false;

        public async Task<string> GenerarAsync(ConsultaIA consulta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new CoachIAException("La clave de la API de Gemini no está configurada.");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = consulta.Prompt } } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string responseJson;
            try
            {
                response = await _httpClient.PostAsync(url, content, cancellationToken);
                responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Red caída o timeout: el caso típico de "sin internet". Se lanza para
                // que la cadena lo registre y pase al respaldo.
                throw new CoachIAException("No se pudo contactar a la API de Gemini.", ex);
            }

            return ExtraerTexto(responseJson);
        }

        private static string ExtraerTexto(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "desconocido";
                    throw new CoachIAException($"La API de Gemini devolvió un error: {msg}");
                }

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                    || candidates.GetArrayLength() == 0)
                    throw new CoachIAException("La API de Gemini no devolvió ninguna respuesta.");

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new CoachIAException("La respuesta de Gemini vino vacía.");

                return text;
            }
            catch (JsonException ex)
            {
                throw new CoachIAException("La respuesta de Gemini no se pudo interpretar.", ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new CoachIAException("La respuesta de Gemini no tenía el formato esperado.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new CoachIAException("La respuesta de Gemini no tenía el formato esperado.", ex);
            }
        }
    }
}
