# 02 — Arquitectura y reglas

> Este documento es **normativo**: define qué está permitido y qué no. Cualquier código nuevo (escrito por una persona o generado por IA) debe cumplirlo. Si algo aquí estorba, se cambia el documento con un ADR — no se ignora en silencio.

---

## Arquitectura hexagonal (Ports & Adapters)

La idea en una frase: **el núcleo del negocio no sabe nada del mundo exterior.** No sabe que existe una base de datos, ni HTTP, ni Gemini, ni ASP.NET. El mundo exterior se adapta al núcleo, nunca al revés.

### Regla de dependencias

```
        ┌─────────────────────────────────────────┐
        │  FitnessCoach (Web)                     │
        │  Controllers, Views, Program.cs         │
        └───────────────┬─────────────────────────┘
                        │ referencia
        ┌───────────────▼──────────┬──────────────┐
        │ FitnessCoach.Application │              │
        │ Servicios / casos de uso │              │
        └───────────────┬──────────┘              │
                        │ referencia              │ referencia
        ┌───────────────▼─────────────────────────▼┐
        │ FitnessCoach.Domain                      │
        │ Modelos, Puertos, Patrones GOF           │
        │ ← NO REFERENCIA A NADIE                  │
        └──────────────────────────────────────────┘
                        ▲
                        │ implementa los puertos
        ┌───────────────┴──────────────────────────┐
        │ FitnessCoach.Infrastructure              │
        │ EF Core, repositorios, Gemini, Identity  │
        └──────────────────────────────────────────┘
```

**Las flechas apuntan hacia adentro. Siempre.**

### Tabla de lo permitido

| Proyecto | PUEDE referenciar | NO puede referenciar |
|----------|-------------------|----------------------|
| `Domain` | *(nada — cero dependencias de proyecto, cero paquetes de framework)* | Todo lo demás |
| `Application` | `Domain` | `Infrastructure`, Web, EF Core, ASP.NET |
| `Infrastructure` | `Domain` (y `Application` si hace falta) | Web |
| `FitnessCoach` (Web) | `Application`, `Infrastructure` | — |
| `Tests` | `Domain`, `Application` | `Infrastructure`, Web *(decisión de ADR-08, ver abajo)* |

### Prohibiciones concretas

Dentro de `FitnessCoach.Domain`, **nunca** debe aparecer:

- ❌ `using Microsoft.EntityFrameworkCore;`
- ❌ `using Microsoft.AspNetCore.*;`
- ❌ `using Microsoft.AspNetCore.Identity;`
- ❌ Atributos de EF (`[Table]`, `[Column]`, `[Key]`) — el mapeo va en `OnModelCreating`
- ❌ `HttpClient`, `IConfiguration`, o cualquier tipo de infraestructura

**Cómo se resuelve la tensión típica:** cuando el dominio necesita "saber" algo del exterior, se usa un **tipo primitivo opaco**, no el tipo del framework.

> Ejemplo real: para vincular un `UsuarioPerfil` con un usuario autenticado de ASP.NET Identity, `Domain` guarda un `string? IdentityUserId` — **no** un `ApplicationUser`. El dominio guarda "un identificador de a quién pertenece esto"; no sabe ni le importa que del otro lado haya un `IdentityUser` con hash de contraseña.

### Por qué `Tests` no referencia `Infrastructure`

Decisión formal del ADR-08. Las clases con la lógica de negocio sensible (Strategy, Decorator, Factory, cálculo calórico) no dependen de EF Core ni de Gemini. Manteniendo esa frontera, la suite corre **en segundos, sin base de datos ni API key** — que es lo que permite que el pipeline de CI funcione en un runner limpio de GitHub sin secretos ni infraestructura.

Si en el futuro se quiere probar `Infrastructure`, va en un **proyecto de pruebas separado** (`FitnessCoach.IntegrationTests`), no mezclado con el actual, y probablemente en un job distinto del pipeline.

---

## Dónde va cada cosa

| Si estás escribiendo… | Va en… | Ejemplo existente |
|----------------------|--------|-------------------|
| Una entidad o concepto del negocio | `Domain/Models/` | `UsuarioPerfil`, `Rutina`, `PlanAlimentacion` |
| Una interfaz que el dominio necesita que "alguien" implemente | `Domain/Ports/` | `IRepositorioUsuario` |
| Un patrón GOF de negocio | `Domain/Patterns/` | `EstrategiaPerderPeso`, `RutinaConCalentamiento` |
| Un caso de uso / orquestación | `Application/Services/` | `GeneradorRutinasService` |
| Una implementación concreta de un puerto | `Infrastructure/Repositories/` | `RepositorioUsuarioMemoria` |
| Acceso a un servicio externo | `Infrastructure/Adapters/` | `GeminiCoachService` |
| Mapeo de base de datos | `Infrastructure/Data/` | `ApplicationDbContext` |
| Recibir HTTP y devolver una vista | `Controllers/` | `PerfilController` |
| Recibir HTTP y devolver JSON | `Web/ApiControllers/` | `UsuariosApiController` |
| Registro de dependencias | `Program.cs` | — |

**Regla de oro para el controlador:** un controlador recibe la petición, la valida, llama a **un** servicio de `Application`, y devuelve el resultado. Si un controlador tiene lógica de negocio (cálculos, `switch` de reglas, decisiones sobre datos), esa lógica está en el lugar equivocado.

---

## Patrones GOF en uso

Estos tres están implementados, documentados en el ADR-06 y **cubiertos por pruebas** (ADR-08). Cualquier cambio que los rompa debe hacer fallar la suite.

### Strategy — selección de comportamiento por objetivo

- **Rutinas:** `IEstrategiaRutina` ← `EstrategiaPerderPeso`, `EstrategiaGanarMusculo`, `EstrategiaRecomposicion`
- **Alimentación:** `IEstrategiaAlimentacion` ← `AlimentacionPerderPeso`, `AlimentacionGanarMusculo`, `AlimentacionRecomposicion`
- La selección ocurre en `GeneradorRutinasService` / `GeneradorAlimentacionService` mediante un `switch` sobre el tipo de `ObjetivoFitness`.

### Decorator — envolver sin modificar

- `RutinaDecorator` (abstracto) ← `RutinaConCalentamiento`, `RutinaConEnfriamiento`
- `PlanConHidratacion` para alimentación
- **Composición actual:** `new RutinaConEnfriamiento(new RutinaConCalentamiento(estrategia))` → el calentamiento queda primero, el enfriamiento al final. **El orden importa y está probado.**

### Factory Method — reconstruir el objetivo desde la BD

- `ObjetivoFitnessFactory.CrearPorNombre(string?)` / `.ObtenerNombreTipo(ObjetivoFitness?)`
- **Por qué existe:** `ObjetivoFitness` es una clase abstracta sin datos propios; no es mapeable a una columna. Se persiste el nombre del tipo concreto (`ObjetivoActualTipo`) y se reconstruye al leer, vía `HasConversion` en `OnModelCreating`.
- ⚠ **Mantenimiento manual:** cada objetivo nuevo debe agregarse *a la clase concreta y al `switch` del factory*. Es la fragilidad conocida de este patrón aquí, ya documentada en ADR-07.

### Patrones planificados (aún no implementados)

- **Factory / Strategy para proveedores de IA** — para que si Gemini falla, se pueda caer a otro proveedor. Ver Fase 6 del roadmap.

---

## Convenciones de código

- **Idioma:** el dominio se nombra **en español** (`UsuarioPerfil`, `GenerarRutina`, `ObjetivoActual`). Los tipos de framework quedan en inglés porque no son nuestros. Es consistente con todo el código existente; no mezclar.
- **Nullable habilitado** (`<Nullable>enable</Nullable>`) en los 4 proyectos. No suprimir warnings con `!` sin una razón que se pueda explicar.
- **ImplicitUsings habilitado.**
- **Un tipo público por archivo**, y el nombre del archivo debe coincidir exactamente con el tipo. *(Hoy hay tres violaciones de esto — ver deudas D-10 a D-12.)*
- **Codificación de archivos: UTF-8.** *(Hoy hay archivos en ISO-8859-1 con caracteres corruptos — ver deuda D-14.)*

---

## Cómo agregar una funcionalidad sin romper la arquitectura

Receta paso a paso, en este orden:

1. **¿Es un concepto del negocio?** → modelo en `Domain/Models/`.
2. **¿Necesita algo del exterior?** (guardar, leer, llamar a una API) → define la interfaz en `Domain/Ports/`.
3. **¿Hay una regla de negocio?** → si es una variación intercambiable, es un Strategy en `Domain/Patterns/`; si es orquestación de varios pasos, es un servicio en `Application/Services/`.
4. **Implementa el puerto** en `Infrastructure/`.
5. **Registra la dependencia** en `Program.cs`.
6. **Escribe la prueba** en `Tests/` — de lo que está en `Domain` y `Application`.
7. **Expón por HTTP** en `Controllers/` (vista) o `Web/ApiControllers/` (JSON).
8. **Si tomaste una decisión de arquitectura no obvia** → ADR nuevo.

Si en el paso 4 te das cuenta de que necesitas modificar `Domain` para que la infraestructura funcione, **detente**: eso es señal de que la abstracción del puerto está mal diseñada.
