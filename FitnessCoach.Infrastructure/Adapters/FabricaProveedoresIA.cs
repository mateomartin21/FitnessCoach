using FitnessCoach.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace FitnessCoach.Infrastructure.Adapters
{
    /// <summary>
    /// Arma la lista ordenada de proveedores de IA a partir de la configuración.
    ///
    /// Orden pensado para la resiliencia:
    ///   1. Gemini (modelo principal) — el primario.
    ///   2. Groq / OpenRouter, si hay clave — respaldo de OTRA empresa, que sobrevive a
    ///      una caída de Google. Son gratuitos; se activan solos al configurar la clave.
    ///   3. Gemini (modelo secundario) — reintento barato ante un problema puntual del
    ///      primer modelo.
    ///
    /// Todo lo que necesita clave se omite en silencio si no la hay: la app arranca
    /// igual y la cadena cae al siguiente proveedor o, en última instancia, al offline.
    /// </summary>
    public class FabricaProveedoresIA : IFabricaProveedoresIA
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public FabricaProveedoresIA(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public IReadOnlyList<IProveedorIA> CrearProveedores()
        {
            var proveedores = new List<IProveedorIA>();

            var geminiKey = _config["Gemini:ApiKey"];
            var modeloPrincipal = _config["Gemini:Model"] ?? "gemini-2.0-flash";
            var modeloSecundario = _config["Gemini:ModelFallback"] ?? "gemini-2.0-flash-lite";

            if (!string.IsNullOrWhiteSpace(geminiKey))
                proveedores.Add(new GeminiCoachService(Cliente(), geminiKey, modeloPrincipal));

            // Respaldo de otra empresa (gratuito): Groq y OpenRouter hablan el mismo
            // protocolo, así que el mismo adaptador sirve para los dos.
            AgregarOpenAICompatible(proveedores, "Groq",
                baseUrlPorDefecto: "https://api.groq.com/openai/v1",
                modeloPorDefecto: "llama-3.3-70b-versatile");

            AgregarOpenAICompatible(proveedores, "OpenRouter",
                baseUrlPorDefecto: "https://openrouter.ai/api/v1",
                modeloPorDefecto: "meta-llama/llama-3.3-70b-instruct:free");

            // El segundo modelo de Gemini, como reintento final antes del offline.
            if (!string.IsNullOrWhiteSpace(geminiKey) && modeloSecundario != modeloPrincipal)
                proveedores.Add(new GeminiCoachService(Cliente(), geminiKey, modeloSecundario));

            return proveedores;
        }

        private void AgregarOpenAICompatible(List<IProveedorIA> proveedores, string nombre, string baseUrlPorDefecto, string modeloPorDefecto)
        {
            var key = _config[$"{nombre}:ApiKey"];
            if (string.IsNullOrWhiteSpace(key)) return;

            var baseUrl = _config[$"{nombre}:BaseUrl"] ?? baseUrlPorDefecto;
            var modelo = _config[$"{nombre}:Model"] ?? modeloPorDefecto;

            proveedores.Add(new ProveedorOpenAICompatible(Cliente(), key, baseUrl, modelo, nombre));
        }

        private HttpClient Cliente() => _httpFactory.CreateClient("ia");
    }
}
