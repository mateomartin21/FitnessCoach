# FitnessCoach

Aplicación web desarrollada con ASP.NET Core MVC (.NET 10) como proyecto académico para el curso de Arquitectura de Software — Tecnológico de Software.

**Estado:** las diez fases del roadmap están cerradas (ADR-20, 30/07/2026). 348 pruebas en verde, sin deuda técnica crítica ni alta abierta.

---

## Descripción

FitnessCoach es un entrenador personal con IA. A partir del perfil y el objetivo del usuario compone su **rutina** desde un catálogo de 1323 ejercicios y su **plan de comidas** desde un catálogo de 67 alimentos con macros reales, y le da un tracker para registrar entrenamientos, peso, récords y lo que come. Encima de todo eso vive **Koda**, el coach: responde en el chat y analiza el progreso, la dieta, la rutina y la semana con los datos reales del usuario, nunca inventados.

---

## Funcionalidades

| Área | Qué hace |
|------|----------|
| **Cuentas** | Registro, login y logout con ASP.NET Identity. Los datos de cada usuario están aislados: el dueño se resuelve desde la identidad, nunca desde la URL. |
| **Perfil** | Datos físicos, objetivo, zona horaria (autodetectada del navegador) y calorías diarias por Mifflin-St Jeor. |
| **Rutina** | Compuesta por objetivo desde el catálogo, con calentamiento y enfriamiento automáticos, GIF, instrucciones y técnica de cada ejercicio. |
| **Alimentación** | Macros calculados (proteína por peso, grasa por % del total, carbohidratos por diferencia), plan de comidas armado desde el catálogo, sustituciones por equivalencia de macros y preferencias (dietas y alimentos excluidos). |
| **Diario** | Registro de lo comido por día, con barras contra el objetivo. |
| **Progreso** | Historial de peso con gráfica, entrenamientos completados, rachas y récords personales por ejercicio. |
| **Logros** | Nivel y XP, 12 logros y 3 misiones semanales, **derivados de los hechos del tracker** (sin estado de juego persistido). |
| **Koda (IA)** | Chat y análisis por pantalla. Cadena de proveedores con respaldo: si ninguno responde, contesta un coach offline por reglas. |
| **API REST** | `/api/perfil` documentada con OpenAPI + Scalar. Cubre perfil, calorías, historial de peso (alta, edición, borrado) y entrenamientos con rachas. |

---

## Arquitectura

**Hexagonal (Ports & Adapters)** en cuatro proyectos. La regla que la sostiene: las dependencias apuntan **hacia** el dominio.

```
FitnessCoach.Domain          # Modelos, puertos, cálculo puro y patrones GOF. No referencia a nadie.
FitnessCoach.Application     # Servicios de aplicación y casos de uso. Solo referencia Domain.
FitnessCoach.Infrastructure  # Adaptadores: EF Core + SQL Server, Identity, proveedores de IA.
FitnessCoach (raíz)          # Controladores MVC y API, vistas Razor. Program.cs es el composition root.
FitnessCoach.Tests           # xUnit, sin Moq (dobles a mano). No referencia Infrastructure (ADR-08).
```

Los diagramas C4 (contexto, contenedores y componentes, en Mermaid) están en
**[docs/diagrama-arquitectura.md](docs/diagrama-arquitectura.md)**.

### Patrones GOF en uso

| Patrón | Dónde | Para qué |
|--------|-------|----------|
| **Strategy** | `Domain/Patterns/Strategy/` | Una estrategia por objetivo, tanto de rutina como de alimentación. |
| **Decorator** | `Domain/Patterns/Decorator/` y `Infrastructure/Repositories/*EnCache` | Calentamiento y enfriamiento sobre cualquier rutina; y la caché de catálogos, que envuelve al adaptador SQL implementando el mismo puerto. |
| **Factory Method** | `ObjetivoFitnessFactory`, `FabricaProveedoresIA` | Reconstruir el objetivo desde la BD; armar los proveedores de IA según la configuración disponible. |
| **Chain of Responsibility** | `Application/Coaching/CoachResiliente` | Recorrer los proveedores de IA hasta que uno responda, con el offline como último eslabón que no puede fallar. |

---

## Tecnologías

- ASP.NET Core MVC + Web API (.NET 10)
- Entity Framework Core + SQL Server (LocalDB en desarrollo, Express al desplegar)
- ASP.NET Core Identity
- Razor Views + Bootstrap 5, con un sistema de diseño pixel art propio en tokens CSS
- JavaScript vanilla, sin librerías, para la capa de vida de Koda y las medallas en canvas
- OpenAPI + Scalar UI
- xUnit (348 pruebas) y GitHub Actions

---

## Cómo correrlo

```bash
# 1. Restaurar y compilar
dotnet restore
dotnet build

# 2. Crear la base (LocalDB por defecto, ver appsettings.json)
dotnet ef database update --project FitnessCoach.Infrastructure --startup-project FitnessCoach.csproj

# 3. Levantar
dotnet run --project FitnessCoach.csproj
```

Los catálogos de ejercicios y alimentos se siembran solos al arrancar, desde los JSON de
`FitnessCoach.Infrastructure/Data/`.

### Claves de IA (opcionales)

Sin ninguna clave la app funciona: Koda responde con el coach offline por reglas. Para usar los proveedores reales,
en user-secrets (nunca en `appsettings.json`):

```bash
dotnet user-secrets set "Gemini:ApiKey" "tu-clave"
dotnet user-secrets set "Groq:ApiKey" "tu-clave"     # respaldo de otra empresa, capa gratuita
```

### Pruebas

```bash
dotnet test
```

---

## Documentación

El set de contexto vive en [docs/contexto/](docs/contexto/) y se mantiene al día fase por fase:

| Documento | Contenido |
|-----------|-----------|
| [00-INDICE.md](docs/contexto/00-INDICE.md) | Cómo usar el set y estado actual |
| [01-PROYECTO.md](docs/contexto/01-PROYECTO.md) | Qué hay hecho, qué no, e historial de ADRs |
| [02-ARQUITECTURA.md](docs/contexto/02-ARQUITECTURA.md) | Reglas de dependencias y patrones |
| [03-ESTANDARES.md](docs/contexto/03-ESTANDARES.md) | Convenciones, accesibilidad, fechas y la prueba de fuego |
| [04-DEUDA-TECNICA.md](docs/contexto/04-DEUDA-TECNICA.md) | Inventario honesto de lo que está mal |
| [05-VISION-PRODUCTO.md](docs/contexto/05-VISION-PRODUCTO.md) | Hacia dónde va el producto |
| [06-ROADMAP.md](docs/contexto/06-ROADMAP.md) | Las diez fases, cerradas |

Los **20 ADR** están en [docs/](docs/): del ADR-01 al ADR-06 documentan las decisiones iniciales, y del ADR-07 al ADR-20 cierra uno por cada fase del roadmap.

---

## Despliegue

Se despliega con **SQL Server Express**, que no tiene costo de licencia (límite de 10 GB por base, de sobra para esta
app) y no requiere ningún cambio de código. Detrás de un proxy o balanceador hay que declarar los proxies de confianza
en configuración, para que el límite de intentos por IP cuente al cliente real y no al balanceador:

```json
"ForwardedHeaders": { "KnownProxies": ["10.0.0.1"], "KnownNetworks": ["10.0.0.0/8"] }
```

PostgreSQL queda como alternativa registrada (D-35), con una advertencia: **PostgreSQL distingue mayúsculas y SQL Server
no**, así que las migraciones hay que regenerarlas con Npgsql, no editarlas.

---

## Ramas del repositorio

| Rama | Descripción |
|------|-------------|
| `CD/CI` | Rama de integración: el estado real y más avanzado del proyecto. |
| `fase-N/...` | Una rama por fase del roadmap, cada una con su PR y su ADR. |
| `master` | Quedó atrás respecto de `CD/CI`. |
| `mvc-inicial`, `api-layer` | Estados históricos: MVC puro y la incorporación de la capa API. |

---

## Cláusula de Uso de IA

Este proyecto fue desarrollado con asistencia de herramientas de inteligencia artificial (Claude — Anthropic). El
trabajo se organizó por fases: para cada una se acordó un plan, la IA aplicó los cambios y el autor revisó el diff antes
de integrarlo. Las decisiones de arquitectura, alcance y producto —incluidas las documentadas en los ADRs— son del
autor, igual que el arte del personaje. Varias decisiones registradas en los ADRs salieron de correcciones del autor
sobre lo propuesto por la IA.

---

*Tecnológico de Software — Desarrollo de Software y Negocios Digitales*
*Arquitectura de Software — Dr. Pedrozo — 2026*
