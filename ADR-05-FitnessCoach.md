# ADR-05: Incorporación de API REST a FitnessCoach

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 19/06/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** Este ADR extiende el ADR-04 (Arquitectura Hexagonal uniproyecto). Los Controllers API se incorporan como un nuevo tipo de Adapter de entrada dentro de la carpeta `Web/ApiControllers/`, respetando la decisión de uniproyecto ya documentada.

---

## Contexto

FitnessCoach contaba hasta esta semana con una única interfaz de usuario: la aplicación web MVC accesible desde el navegador. Esta arquitectura cubre el caso de uso de un usuario que interactúa con el sistema desde una computadora, pero deja sin respuesta dos necesidades planteadas por el escenario del sistema:

- **Acceso desde dispositivos móviles:** una app móvil no puede consumir vistas Razor — necesita datos en JSON.
- **Integración con otros sistemas:** un sistema externo (por ejemplo, un bot de recordatorios o un dashboard de seguimiento) tampoco puede consumir HTML renderizado.

La arquitectura hexagonal adoptada en el ADR-04 anticipaba este momento: `CitasApp.Application` (en nuestro caso, los servicios de dominio de FitnessCoach) ya contenía toda la lógica de negocio desacoplada de cualquier interfaz. Agregar una API REST es simplemente agregar un nuevo Adapter de entrada que consume esos mismos servicios.

La actividad académica de la Semana 7 formaliza esta extensión y exige además documentación profesional de los endpoints mediante una herramienta estándar de la industria.

---

## Decisión

Se incorpora una **API REST con ASP.NET Core Web API** al proyecto FitnessCoach, expuesta en la ruta `/api/`, con documentación interactiva generada mediante **Scalar** sobre el estándar **OpenAPI 3.1**.

Los nuevos API Controllers se ubican en `Web/ApiControllers/`, manteniendo la decisión de uniproyecto del ADR-04.

### Endpoints implementados

**UsuariosApi** — gestión de perfiles de usuario:

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/usuarios/{id}` | Obtiene el perfil de un usuario por ID |
| `POST` | `/api/usuarios` | Crea un nuevo perfil de usuario |
| `GET` | `/api/usuarios/{id}/calorias` | Calcula las calorías diarias recomendadas |

**ProgresoApi** — historial de progreso de peso:

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/usuarios/{usuarioId}/progreso` | Obtiene el historial completo ordenado por fecha |
| `GET` | `/api/usuarios/{usuarioId}/progreso/ultimo` | Obtiene el registro más reciente |
| `POST` | `/api/usuarios/{usuarioId}/progreso` | Agrega un nuevo registro de peso |

### Documentación

La UI de la API está disponible en `/scalar/v1` cuando el proyecto corre localmente. El documento OpenAPI en formato JSON se genera automáticamente en `/openapi/v1.json`.

### Decisión de persistencia en producción

El Adapter actual (`RepositorioUsuarioMemoria`) almacena los datos en memoria — cada reinicio del servidor pierde el historial. Para el despliegue en producción (Semana 8, EC2 en AWS), se migrará a **PostgreSQL vía Entity Framework Core**, creando un nuevo Adapter `RepositorioUsuarioPostgres` que implementa `IRepositorioUsuario`. El cambio de Adapter se registra en `Program.cs` sin modificar ningún Controller ni servicio de dominio — exactamente el beneficio que la arquitectura hexagonal garantiza.

---

## Diagrama

```
Adapters de entrada
┌─────────────────────┐    ┌──────────────────────────┐
│  Web/Controllers/   │    │  Web/ApiControllers/     │
│  (MVC — navegador)  │    │  UsuariosApiController   │
│  PerfilController   │    │  ProgresoApiController   │
│  ProgresoController │    │                          │
└────────┬────────────┘    └────────────┬─────────────┘
         │                              │
         └──────────────┬───────────────┘
                        ▼
              Domain/Services/
         CalculadorCaloricoService
         GeneradorRutinasService
                        │
                        ▼
            Infrastructure/
         RepositorioUsuarioMemoria
         (→ RepositorioUsuarioPostgres en prod)
```

---

## Alternativas Consideradas

### Alternativa 1: Minimal APIs de .NET

.NET 10 ofrece Minimal APIs — endpoints definidos directamente en `Program.cs` sin controllers:

```csharp
app.MapGet("/api/usuarios/{id}", (int id, IRepositorioUsuario repo) => ...);
```

**Por qué se descarta:**  
Para dos entidades con tres endpoints cada una, Minimal APIs funcionaría. Sin embargo, los Controllers organizan mejor el código a medida que crece el número de endpoints, permiten agrupar lógica relacionada y son el patrón que el curso trabaja explícitamente. La consistencia con lo ya aprendido y documentado pesa más que la brevedad de Minimal APIs en este contexto académico.

### Alternativa 2: Swagger UI con Swashbuckle

Swashbuckle es la librería de Swagger más conocida para ASP.NET Core.

**Por qué se descarta:**  
Swashbuckle no tiene soporte oficial para .NET 10 — al intentar instalarlo el proyecto falla en tiempo de ejecución con un `TypeLoadException`. Scalar es el reemplazo moderno recomendado para .NET 9/10, lee el mismo estándar OpenAPI 3.1 y produce una UI equivalente o superior. La decisión de usar Scalar sobre Swashbuckle es técnica, no de preferencia.

### Alternativa 3: Proyecto separado FitnessCoach.Api

Crear un segundo proyecto `.csproj` exclusivo para la API, como se hace en CitasApp en la práctica de clase.

**Por qué se descarta:**  
El ADR-04 documentó la decisión de uniproyecto con argumentos que siguen siendo válidos: overhead de configuración de referencias entre proyectos, complejidad de DI entre ensamblados y despliegue como un único ejecutable en Render.com. Los principios hexagonales se aplican igualmente con carpetas dentro de un solo proyecto. Crear un segundo proyecto solo para la API contradiría una decisión ya documentada sin que haya cambiado ninguna de las condiciones que la motivaron.

---

## Consecuencias

### ✅ Lo que gano

- **Los servicios de FitnessCoach son consumibles desde cualquier cliente:** una app móvil, un script de automatización o Postman pueden interactuar con el sistema sin pasar por la interfaz web.
- **La lógica de negocio no cambió:** `CalculadorCaloricoService` y `RepositorioUsuarioMemoria` son exactamente los mismos que usa la interfaz MVC. El ADR-04 garantizaba esto.
- **Documentación profesional automática:** Scalar genera la referencia de la API a partir del código — no hay documentación manual que pueda quedar desactualizada.
- **Base para el despliegue en AWS:** una API REST es el contrato de comunicación estándar entre el backend en EC2 y cualquier cliente futuro.

### ⚠️ Lo que sacrifico o asumo

- **Estado en memoria:** los datos siguen sin persistir entre reinicios. Esto es una limitación conocida y documentada, pendiente de resolver en la Semana 8 con PostgreSQL.
- **Sin autenticación:** los endpoints son públicos. Para producción real habría que agregar JWT o API Keys, pero está fuera del alcance de esta entrega académica.
- **Scalar en lugar de Swagger UI clásico:** aunque funcionalmente equivalente, Scalar puede ser menos familiar para quienes conocen solo la interfaz de Swashbuckle.

---
