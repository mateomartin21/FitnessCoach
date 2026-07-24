# 01 — El proyecto

## Qué es

**FitnessCoach** es una aplicación web de entrenador personal. Genera rutinas de entrenamiento y planes de alimentación personalizados según el objetivo fitness del usuario, calcula sus calorías diarias recomendadas, y ofrece un asistente de IA conversacional llamado **Lobo Coach**.

Nace como proyecto académico del curso de **Arquitectura de Software** (Tecnológico de Software, TSU en Desarrollo e Innovación de Software, 3° cuatrimestre), pero el objetivo declarado es que **no se sienta como un proyecto genérico de escuela** — ver `05-VISION-PRODUCTO.md`.

---

## Stack

| Componente | Tecnología | Versión |
|------------|-----------|---------|
| Runtime | .NET | 10.0 (`net10.0`) |
| Framework web | ASP.NET Core MVC + API Controllers | 10.x |
| ORM | Entity Framework Core | 10.0.10 |
| Base de datos | SQL Server LocalDB | `(localdb)\mssqllocaldb`, BD `FitnessCoachDb` |
| Pruebas | xUnit | via plantilla `dotnet new xunit` |
| CI | GitHub Actions | `ubuntu-latest`, `actions/setup-dotnet@v4` |
| Documentación API | OpenAPI nativo de .NET + Scalar | `Scalar.AspNetCore` 2.16.4 |
| IA | Google Gemini | `gemini-2.5-flash`, endpoint `v1beta` |
| Frontend | Razor Views + Bootstrap + jQuery | Bootstrap 5 (en `wwwroot/lib`) |
| Entorno de desarrollo | Visual Studio 2026 en Windows, Developer PowerShell | — |

---

## Estructura de la solución

Solución: `FitnessCoach.slnx` (formato nuevo XML, no `.sln` clásico). **5 proyectos:**

```
FitnessCoach/                          ← raíz del repo Y del proyecto web
│
├── FitnessCoach.csproj                ← proyecto WEB (Sdk.Web)
├── Program.cs                         ← composition root (DI, pipeline HTTP)
├── Controllers/                       ← controladores MVC (vistas)
├── Web/ApiControllers/                ← controladores REST (JSON)
├── Views/                             ← vistas Razor
├── wwwroot/                           ← estáticos (css, js, imágenes, libs)
│
├── FitnessCoach.Domain/               ← núcleo: modelos, puertos, patrones GOF
│   ├── Models/
│   ├── Ports/
│   └── Patterns/  (Strategy, Decorator)
│
├── FitnessCoach.Application/          ← servicios de aplicación / casos de uso
│   └── Services/
│
├── FitnessCoach.Infrastructure/       ← adaptadores: EF Core, Gemini, repositorios
│   ├── Data/  (ApplicationDbContext, Migrations)
│   ├── Repositories/
│   └── Adapters/  (GeminiCoachService)
│
├── FitnessCoach.Tests/                ← 21 pruebas xUnit
│   ├── Services/
│   ├── Objetivos/
│   ├── Patterns/
│   └── ServicesIntegration/
│
├── .github/workflows/ci.yml           ← pipeline de CI
└── docs/                              ← ADRs y esta carpeta de contexto
```

### ⚠ Particularidad importante de la estructura

El proyecto web (`FitnessCoach.csproj`) vive **en la misma carpeta raíz** que las carpetas de los demás proyectos. Por eso su `.csproj` contiene exclusiones explícitas:

```xml
<Compile Remove="FitnessCoach.Domain\**" />
<Compile Remove="FitnessCoach.Application\**" />
<Compile Remove="FitnessCoach.Infrastructure\**" />
<Compile Remove="FitnessCoach.Tests\**" />
<!-- lo mismo para Content y EmbeddedResource -->
```

**Regla:** si algún día se agrega un sexto proyecto en la raíz, hay que sumarlo a esas **tres** listas o el proyecto web intentará compilar sus archivos y fallará con errores de tipo no encontrado.

---

## Comandos

Todos desde la **raíz del repo**, en Developer PowerShell:

```powershell
# Compilar toda la solución
dotnet build FitnessCoach.slnx

# Correr las pruebas (21 actualmente)
dotnet test FitnessCoach.Tests/FitnessCoach.Tests.csproj

# Ejecutar la app
dotnet run

# Migraciones de EF Core (el DbContext vive en Infrastructure,
# pero el proyecto de arranque es el web)
dotnet ef migrations add <NombreMigracion> --project FitnessCoach.Infrastructure --startup-project .
dotnet ef database update --project FitnessCoach.Infrastructure --startup-project .

# Configurar la API key de Gemini (NUNCA en appsettings.json)
dotnet user-secrets set "Gemini:ApiKey" "<la-key>"
```

---

## Ramas

| Rama | Propósito |
|------|-----------|
| `master` | **Rama por defecto** del repo en GitHub |
| `deuda-tecnica` | Rama de trabajo principal actual (persistencia EF Core, ADR-07) |
| `CD/CI` | Suite de pruebas + pipeline CI (ADR-08) — PR #1 abierto contra `deuda-tecnica` |
| `mvc-inicial` | Histórica — primera versión MVC monolítica |
| `api-layer` | Histórica — introducción de la capa API |
| `docs/diagrama-arquitectura` | Histórica — diagramas C4 |

**Nota sobre GitHub Actions:** un workflow solo se dispara si el archivo está exactamente en `.github/workflows/`. Además, el commit que agrega un workflow **por primera vez** no dispara una corrida para sí mismo — hace falta un push posterior. Ambos puntos ya causaron confusión una vez (documentado en ADR-08).

---

## Estado real del proyecto

> Esta sección es deliberadamente honesta. Ver `04-DEUDA-TECNICA.md` para el detalle.

### ✅ Funciona y está verificado

- Arquitectura hexagonal multiproyecto, con las 4 capas separadas y compilando.
- Patrones GOF implementados y **probados**: Strategy (rutinas y alimentación), Decorator (calentamiento, enfriamiento, hidratación), Factory Method (`ObjetivoFitnessFactory`).
- Cálculo calórico (Mifflin-St Jeor + multiplicador por objetivo).
- Integración con Gemini funcionando, con timeout de 15s y la API key fuera del repo (user-secrets).
- 21 pruebas xUnit en verde.
- Pipeline de CI corriendo en cada push y PR, con check verde confirmado.
- `ApplicationDbContext` + migración `InitialCreate` existen y están bien modelados.
- **Persistencia real conectada (Fase 1, ADR-09).** `RepositorioUsuarioSql` (`Scoped`) consume el `DbContext`; los datos sobreviven al reinicio del servidor. Se conserva `RepositorioUsuarioMemoria` como segundo adaptador del puerto.

### ❌ Existe pero NO está conectado / no funciona como se documentó

- **No hay autenticación.** Toda la app opera sobre un usuario fijo con `Id = 1`, hardcodeado en `PerfilController`, `ProgresoController` e `IaCoachController`.
- **Los endpoints REST están completamente abiertos**, sin autenticación ni autorización.
- `Program.cs` llama a `UseAuthorization()` pero nunca a `UseAuthentication()` — la línea no hace nada.

### ⏳ No existe todavía

Login, tracker real, catálogo de ejercicios, GIFs, gamificación, fallback de IA, rediseño visual. → Ver `06-ROADMAP.md`.

---

## Historial de ADRs

| ADR | Tema |
|-----|------|
| ADR-01 | Decisión inicial del proyecto |
| ADR-03 | Dirección hacia arquitectura hexagonal |
| ADR-04 | Estructura de proyecto único con SOLID + repositorio en memoria |
| ADR-05 | (patrones / evolución) |
| ADR-06 | Arquitectura hexagonal multiproyecto + 3 patrones GOF |
| ADR-07 | Deuda técnica: persistencia EF Core + SQL Server, seguridad de la API key de Gemini |
| ADR-08 | Suite de pruebas xUnit + pipeline de Integración Continua |
| ADR-09 | Cierre de la deuda de persistencia: adaptador SQL real para `IRepositorioUsuario` |

**Convención establecida:** cada ADR abre citando explícitamente su relación con el anterior ("Este ADR extiende el ADR-N…"). Cada uno tiene: Contexto → Decisión → Alternativas Consideradas → Consecuencias → Estado actual.

⚠ Los archivos `docs/ADR-07-*.md` están guardados con markdown escapado (`\#`, `\*\*`) y no renderizan bien en GitHub. Ver deuda D-16.
