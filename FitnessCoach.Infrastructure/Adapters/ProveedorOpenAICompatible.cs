using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Infrastructure.Adapters
{
    /// <summary>
    /// Proveedor de IA que habla el protocolo de OpenAI (<c>/chat/completions</c>).
    ///
    /// Un solo adaptador cubre varios servicios gratuitos de otras empresas —Groq,
    /// OpenRouter— porque todos exponen la misma forma de API: solo cambia la URL base,
    /// el modelo y la clave. Es el respaldo "de otra empresa" que sobrevive a una caída
    /// de Google, y se activa apenas hay una clave configurada.
    ///
    /// Mismo contrato que el resto: recibe el prompt ya armado y lanza
    /// <see cref="CoachIAException"/> ante cualquier fallo.
    /// </summary>
    public class ProveedorOpenAICompatible : IProveedorIA
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;

        public ProveedorOpenAICompatible(HttpClient httpClient, string apiKey, string baseUrl, string model, string nombre)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _baseUrl = baseUrl.TrimEnd('/');
            _model = model;
            Nombre = nombre;
        }

        public string Nombre { get; }
        public bool EsRespaldo => false;

        public async Task<string> GenerarAsync(ConsultaIA consulta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new CoachIAException($"La clave de {Nombre} no está configurada.");

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = consulta.Prompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            string responseJson;
            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new CoachIAException($"No se pudo contactar a {Nombre}.", ex);
            }

            return ExtraerTexto(responseJson);
        }

        private string ExtraerTexto(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "desconocido";
                    throw new CoachIAException($"{Nombre} devolvió un error: {msg}");
                }

                if (!doc.RootElement.TryGetProperty("choices", out var choices)
                    || choices.GetArrayLength() == 0)
                    throw new CoachIAException($"{Nombre} no devolvió ninguna respuesta.");

                var text = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new CoachIAException($"La respuesta de {Nombre} vino vacía.");

                return text;
            }
            catch (JsonException ex)
            {
                throw new CoachIAException($"La respuesta de {Nombre} no se pudo interpretar.", ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new CoachIAException($"La respuesta de {Nombre} no tenía el formato esperado.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new CoachIAException($"La respuesta de {Nombre} no tenía el formato esperado.", ex);
            }
        }
    }
}
