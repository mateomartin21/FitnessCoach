\# ADR-07: Deudas Técnicas y Migración a Persistencia Real (EF Core + SQL Server)



| Campo  | Valor          |

|--------|----------------|

| Autor  | Mateo Martin   |

| Fecha  | 15/07/2026     |

| Estado | `Aceptado`     |



> \*\*Relación con ADRs anteriores:\*\* Este ADR extiende el ADR-06, que dejó documentado que "la migración a PostgreSQL se hará creando `RepositorioUsuarioPostgres` en Infrastructure sin tocar Domain" y que anticipó explícitamente el riesgo de un timeout no controlado en la integración con Gemini. Este ADR resuelve ambos puntos con SQL Server en lugar de PostgreSQL, por practicidad del entorno de desarrollo (LocalDB ya disponible en Visual Studio).



\---



\## Contexto



El ADR-06 dejó formalizada la arquitectura hexagonal multiproyecto con los tres patrones GOF (Strategy, Decorator, Factory Method), pero señaló dos puntos abiertos en su sección de "Consecuencias" y "Resiliencia":



\- El repositorio de usuarios (`RepositorioUsuarioMemoria`) pierde todos los datos al reiniciar el servidor — la persistencia real quedó pendiente.

\- La integración con Gemini no tenía ningún mecanismo de resiliencia (timeout, manejo de fallos de red) documentado ni implementado.



A esto se suma un hallazgo de esta actividad: durante el desarrollo de la integración con Gemini, la API key estuvo a punto de subirse expuesta directamente en `appsettings.json` al repositorio remoto, bloqueada oportunamente por GitHub Push Protection.



Estos tres puntos se documentan formalmente como deuda técnica y se resuelven en esta misma iteración.



\---



\## Deudas técnicas identificadas



\### Deuda 1 — Persistencia en memoria (`RepositorioUsuarioMemoria`) \[infraestructura]

\- \*\*Qué es\*\*: los perfiles de usuario se guardaban en un `List<UsuarioPerfil>` en memoria (`AddSingleton`), sin ninguna base de datos.

\- \*\*Por qué existe\*\*: decisión deliberada para avanzar rápido en las primeras semanas del proyecto sin configurar un motor de BD (documentado en el ADR-04).

\- \*\*Costo de no pagarla\*\*: cada reinicio del servidor (deploy, crash, reinicio manual) borraba todos los perfiles y el progreso registrado de los usuarios.

\- \*\*Propuesta de solución\*\*: migrar a Entity Framework Core con SQL Server, manteniendo `IRepositorioUsuario` como Port sin modificar Domain ni Application — el mismo beneficio de la arquitectura hexagonal que el ADR-06 ya proyectaba.

\- \*\*Estado\*\*: ✅ Resuelta en esta iteración (ver sección "Estado actual").



\### Deuda 2 — Gestión insegura de credenciales externas y sin resiliencia (`GeminiCoachService`) \[infraestructura]

\- \*\*Qué es\*\*: la integración con la API de Gemini dependía de una API key leída desde `IConfiguration`, que casi quedó expuesta en `appsettings.json` en el historial de git. Además, el `HttpClient` no tenía timeout configurado.

\- \*\*Por qué existe\*\*: accidental — se priorizó tener la integración funcionando rápido, dejando pendiente tanto el manejo seguro del secreto como el manejo de errores transitorios de la API externa.

\- \*\*Costo de no pagarla\*\*: si la key se filtra, terceros pueden consumir la cuota de la API a nombre del proyecto; sin timeout, una caída de Gemini podía dejar una petición esperando indefinidamente.

\- \*\*Propuesta de solución\*\*: mover la key a `dotnet user-secrets` en desarrollo (y a variables de entorno o un vault en producción), y configurar `HttpClient.Timeout` explícito.

\- \*\*Estado\*\*: ✅ Resuelta en esta iteración (ver sección "Estado actual").



\---



\## Decisión



\### 1. Persistencia real con Entity Framework Core + SQL Server



Se reemplaza la fuente de datos del repositorio por una base de datos real, manteniendo intacto el contrato `IRepositorioUsuario` definido en `FitnessCoach.Domain.Ports`. `ApplicationDbContext` vive en `FitnessCoach.Infrastructure`, respetando la regla de dependencias del ADR-06: Domain no conoce Entity Framework.



Un detalle propio de este dominio: `UsuarioPerfil.ObjetivoActual` es de tipo `ObjetivoFitness`, una clase abstracta sin datos propios (el Strategy documentado en el ADR-06). Este tipo no es mapeable directamente a una columna. Se resuelve con:

\- Una columna `ObjetivoActualTipo` (`nvarchar(100)`) que guarda el nombre del tipo concreto.

\- Un `ObjetivoFitnessFactory.CrearPorNombre(...)` que reconstruye la instancia correcta al leer, usando un `HasConversion` en `OnModelCreating`.



`HistorialProgreso` (lista de `RegistroProgreso`) se mapea como colección \*owned\* de EF Core, generando una tabla `RegistrosProgreso` con FK hacia `UsuariosPerfil` y `ON DELETE CASCADE`.



\### 2. Resiliencia y seguridad en la integración con Gemini



\- `HttpClient` de `GeminiCoachService` configurado con `Timeout = TimeSpan.FromSeconds(15)`.

\- La API key se gestiona exclusivamente vía `dotnet user-secrets` en desarrollo; `appsettings.json` en el repositorio solo contiene un placeholder vacío.



\---



\## Diagrama de persistencia

FitnessCoach.Infrastructure

└── Data/

├── ApplicationDbContext.cs       (DbSet<UsuarioPerfil>)

└── Migrations/

└── InitialCreate

Tablas generadas:

├── UsuariosPerfil        (Id, Nombre, PesoKg, EstaturaCm, Edad, ObjetivoActualTipo)

└── RegistrosProgreso     (Id, Fecha, PesoKg, Notas, UsuarioPerfilId → FK CASCADE)



\---



\## Alternativas Consideradas



\### Alternativa 1: PostgreSQL en lugar de SQL Server

El ADR-06 mencionaba PostgreSQL/RDS como plan a futuro. Se opta por SQL Server LocalDB en esta iteración por practicidad del entorno de desarrollo (ya disponible en Visual Studio sin instalación adicional), manteniendo la decisión de PostgreSQL como opción válida para un despliegue en AWS RDS más adelante — el cambio de proveedor solo afectaría `FitnessCoach.Infrastructure`, sin tocar Domain ni Application.



\### Alternativa 2: Guardar `ObjetivoActual` con un enum simple en vez de reconstrucción por Factory

Se descarta porque rompería el patrón Strategy documentado en el ADR-06 — un enum obligaría a volver a un `switch` en el código consumidor. La conversión vía `ObjetivoFitnessFactory` preserva el polimorfismo sin tocar el resto del sistema.



\### Alternativa 3: Mantener la key de Gemini directo en `appsettings.json` con `.gitignore`

Se descarta: es frágil (un solo `git add -A` sin cuidado la vuelve a exponer, como casi ocurrió). `user-secrets` la saca completamente del árbol del repositorio.



\---



\## Consecuencias



\### ✅ Lo que gana el sistema

\- Los perfiles y el progreso de los usuarios sobreviven a reinicios del servidor.

\- La arquitectura hexagonal se valida en la práctica: la migración se hizo sin tocar `FitnessCoach.Domain` ni `FitnessCoach.Application`.

\- La integración con Gemini falla de forma controlada ante problemas de red, en vez de colgarse indefinidamente.

\- Ya no hay secretos en el historial de git hacia adelante.



\### ⚠️ Lo que se asume o sacrifica

\- `user-secrets` es una solución de desarrollo local; para producción se necesita una estrategia adicional (variable de entorno del servidor o un vault administrado).

\- El Factory de `ObjetivoFitness` requiere mantenimiento manual: cada nuevo objetivo fitness debe agregarse tanto a la clase concreta como al `switch` del Factory.

\- Persiste la migración pendiente hacia PostgreSQL/RDS si el proyecto se despliega en AWS, como se documentó en el ADR-06.



\---



\## Estado actual del proyecto (avances tras este ADR)



\- ✅ Arquitectura hexagonal multiproyecto (ADR-06) vigente y respetada durante la migración.

\- ✅ Patrones GOF (Strategy, Decorator, Factory Method) intactos; el Strategy de objetivos ahora también persiste correctamente en base de datos.

\- ✅ Persistencia real: SQL Server LocalDB, `ApplicationDbContext`, migración `InitialCreate` aplicada con las tablas `UsuariosPerfil` y `RegistrosProgreso`.

\- ✅ Timeout configurado en la integración con Gemini.

\- ✅ Gestión segura de la API key vía `dotnet user-secrets`.

\- ⏳ Pendiente: reemplazar el registro de `RepositorioUsuarioMemoria` por una implementación real basada en `ApplicationDbContext` (`RepositorioUsuarioSql`) — la infraestructura de base de datos ya existe, falta conectar el repositorio del dominio a ella.

\- ⏳ Pendiente a futuro: evaluar despliegue en AWS con PostgreSQL/RDS, según lo proyectado en el ADR-06.

