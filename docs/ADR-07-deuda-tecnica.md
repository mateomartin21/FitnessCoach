\# ADR-07 — Deudas técnicas identificadas (FitnessCoach)



\## Deuda 1 — Persistencia en memoria (`RepositorioUsuarioMemoria`) \[infraestructura]

\- \*\*Qué es\*\*: los perfiles de usuario se guardan en un `List<UsuarioPerfil>` en memoria (`AddSingleton`), sin ninguna base de datos.

\- \*\*Por qué existe\*\*: decisión deliberada para avanzar rápido en las primeras semanas del proyecto sin configurar un motor de BD.

\- \*\*Costo de no pagarla\*\*: cada vez que la aplicación se reinicia (deploy, crash, reinicio del servidor), se pierden todos los perfiles y el progreso registrado de los usuarios.

\- \*\*Propuesta de solución\*\*: migrar `IRepositorioUsuario` a una implementación con Entity Framework Core (SQL Server o SQLite), siguiendo el mismo patrón de adaptador ya usado en CitasApp.



\## Deuda 2 — Gestión insegura de credenciales externas y sin resiliencia (`GeminiCoachService`) \[infraestructura]

\- \*\*Qué es\*\*: la integración con la API de Gemini depende de una API key externa leída desde `IConfiguration`. Durante el desarrollo esa key casi quedó expuesta directamente en `appsettings.json` y a punto de subirse al repositorio remoto (bloqueado por GitHub Push Protection). Además, el `HttpClient` usado en `GeminiCoachService` no tiene timeout configurado ni política de reintentos ante fallos de red o límites de la API.

\- \*\*Por qué existe\*\*: accidental — se priorizó tener la integración funcionando rápido, dejando pendiente tanto el manejo seguro del secreto como el manejo de errores transitorios de la API externa.

\- \*\*Costo de no pagarla\*\*: si la key se filtra, terceros pueden consumir la cuota de la API a nombre del proyecto; si la API de Gemini falla o tarda (rate limit, caída del servicio), el `IaCoachController` puede quedarse esperando indefinidamente o fallar sin un mensaje claro para el usuario.

\- \*\*Propuesta de solución\*\*: mantener la key exclusivamente en `dotnet user-secrets` (ya aplicado) y en variables de entorno o Azure Key Vault en producción; agregar `HttpClient.Timeout` y una política de reintentos con `Polly` en el registro de `AddHttpClient<GeminiCoachService>()`.

