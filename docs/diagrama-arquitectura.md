# Diagramas de arquitectura C4 — FitnessCoach

Estos diagramas reflejan el estado real del código al cerrar la **Fase 12** (ADR-21).

> **Cómo leerlos.** Cada diagrama responde **una** pregunta. El nivel 3 no es un mapa de todas las clases —eso es la
> tabla de componentes del final, que se lee mucho mejor— sino tres recorridos: cómo se apilan las capas, cómo se arma
> una rutina y cómo responde Koda.

**Índice**

1. [Nivel 1 — Contexto](#nivel-1--contexto-del-sistema)
2. [Nivel 2 — Contenedores](#nivel-2--contenedores)
3. [Nivel 3.1 — Las capas y la regla de dependencias](#nivel-31--las-capas-y-la-regla-de-dependencias)
4. [Nivel 3.2 — Cómo se arma una rutina](#nivel-32--cómo-se-arma-una-rutina)
5. [Nivel 3.3 — Cómo responde Koda](#nivel-33--cómo-responde-koda)
6. [Inventario de componentes](#inventario-de-componentes)

---

## Nivel 1 — Contexto del sistema

```mermaid
C4Context
    title Nivel 1 - Contexto del sistema

    Person(usuario, "Usuario", "Configura su perfil, sigue su rutina y su plan, registra entrenamientos y consulta a Koda")
    System_Ext(gifs, "jsDelivr / ExerciseGymGifsDB", "GIFs de los ejercicios, del CDN al navegador")

    System(fitnessCoach, "FitnessCoach", "Plataforma web de coaching fitness con IA (ASP.NET Core 10)")
    System_Ext(gemini, "Google Gemini", "Proveedor de IA principal")

    System_Ext(groq, "Groq / OpenRouter", "Proveedor de IA de respaldo")

    Rel(usuario, gifs, "Descarga los GIFs", "HTTPS")
    Rel(usuario, fitnessCoach, "Usa", "HTTPS")
    Rel(fitnessCoach, gemini, "Pide análisis", "HTTPS/JSON")
    Rel(fitnessCoach, groq, "Si Gemini falla", "HTTPS/JSON")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")

    %% Sin esto las etiquetas caen justo sobre la descripcion de la caja de destino.
    UpdateRelStyle(usuario, gifs, $offsetY="-28")
    UpdateRelStyle(fitnessCoach, gemini, $offsetY="-28")
```

**Lo importante:** ningún sistema externo es indispensable. Si los dos proveedores de IA fallan —o no hay red— responde
el coach offline por reglas, dentro del propio proceso. Y los GIFs los pide el **navegador**, no el servidor: si el CDN
no contesta, la vista cae al placeholder local de Koda sin romper la página.

---

## Nivel 2 — Contenedores

```mermaid
C4Container
    title Nivel 2 - Contenedores

    Person(usuario, "Usuario", "Navegador")

    System_Boundary(limite, "FitnessCoach") {
        Container(app, "Aplicación web", "ASP.NET Core 10, MVC + Web API", "Vistas y API en un solo proceso. Program.cs es el composition root")
        Container(cache, "Caché en memoria", "IMemoryCache", "Foto de los catálogos de ejercicios y alimentos")
        ContainerDb(sql, "FitnessCoachDb", "PostgreSQL", "Perfiles, catálogos sembrados, claves de sesión y tablas de Identity")
    }

    System_Ext(ia, "Proveedores de IA", "Gemini, Groq/OpenRouter")

    Rel(usuario, app, "Usa", "HTTPS")
    Rel(app, cache, "Consulta catálogos")
    Rel(cache, sql, "Solo al vencer")
    Rel(app, sql, "Perfiles", "EF Core")
    Rel(app, ia, "Cadena con respaldo offline", "HTTPS/JSON")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
    UpdateRelStyle(app, cache, $offsetY="-30")
```

**Por qué la caché es un contenedor propio:** cambia el patrón de acceso a datos. Los catálogos se pueblan por semilla
al arrancar y nadie los modifica en caliente, así que las consultas por grupo muscular o categoría se responden desde
memoria. Antes, armar una rutina consultaba SQL una vez por bloque de ejercicios.

---

## Nivel 3.1 — Las capas y la regla de dependencias

La única regla que sostiene toda la arquitectura: **las flechas apuntan siempre hacia el dominio.**

```mermaid
flowchart TD
    WEB["<b>Web</b><br/>Controladores MVC y API<br/>Program.cs compone todo"]
    APP["<b>Application</b><br/>Servicios de caso de uso<br/>Coaching de Koda"]
    DOM["<b>Domain</b><br/>Modelos, cálculo puro,<br/>patrones y <b>puertos</b>"]
    INF["<b>Infrastructure</b><br/>EF Core, repositorios SQL,<br/>adaptadores de IA"]

    WEB --> APP
    APP --> DOM
    INF -. "implementan los puertos" .-> DOM
    WEB -. "solo para inyectar" .-> INF

    classDef nucleo fill:#d4edff,stroke:#2f6bff,stroke-width:2px,color:#000;
    class DOM nucleo;
```

`Domain` no referencia a nadie. `Application` solo referencia a `Domain`. `Infrastructure` **implementa** los puertos
que `Domain` declara, y por eso la flecha va al revés de lo que uno esperaría: es la inversión de dependencias.

Esa regla tiene consecuencias concretas y verificables:

- `IndiceEjercicios` y `EquipoEntrenamiento` viven en `Domain`, no en `Application`, porque los usa un adaptador de
  `Infrastructure` —que no referencia `Application`—.
- `FitnessCoach.Tests` **no referencia `Infrastructure`** (ADR-08): todo lo que se prueba se prueba contra puertos, con
  dobles escritos a mano.

---

## Nivel 3.2 — Cómo se arma una rutina

El recorrido completo de una petición a `/Rutinas`, que es donde se ven trabajando juntos los tres patrones.

```mermaid
flowchart LR
    A["RutinasController"] --> B["GeneradorRutinasService"]
    B --> C{"¿Qué objetivo?"}
    C -->|Perder peso| D["EstrategiaPerderPeso"]
    C -->|Ganar músculo| E["EstrategiaGanarMusculo"]
    C -->|Recomposición| F["EstrategiaRecomposicion"]

    D --> G["Elegir ejercicios<br/><i>filtrando por el equipo<br/>del usuario</i>"]
    E --> G
    F --> G

    G --> H["IRepositorioEjercicios<br/><i>(puerto)</i>"]
    H --> I["Caché 12 h<br/><i>(Decorator)</i>"]
    I --> J[("SQL")]

    G --> K["Decorator:<br/>+ calentamiento<br/>+ enfriamiento"]
    K --> L["Aplicar sustituciones<br/>elegidas a mano"]
    L --> M["Rutina lista"]

    classDef patron fill:#fff3cd,stroke:#c90,color:#000;
    classDef puerto fill:#d4edff,stroke:#2f6bff,color:#000;
    class D,E,F,K,I patron;
    class H puerto;
```

Amarillo = patrón de diseño explícito · azul = puerto del dominio.

**El orden importa y es deliberado.** El equipo del usuario filtra **antes** de elegir, porque prescribir algo que no
puede hacer es igual que no prescribir nada. Las sustituciones se aplican **al final**, después de componer y decorar,
porque son una decisión del usuario y no del objetivo: así deshacer una devuelve la prescripción original sin
recalcular nada (ADR-21).

---

## Nivel 3.3 — Cómo responde Koda

```mermaid
flowchart LR
    P["Pregunta<br/>(chat o Analizar)"] --> A["ArmadorContextoCoach<br/>perfil + plan + rutina + diario<br/>+ récords + semana"]
    A --> V["PersonalidadKoda<br/>voz de Koda,<br/>reglas de no inventar"]
    V --> G1["1. Gemini flash"]

    G1 -->|falla| G2["2. Groq llama-3.3-70b<br/><i>(si hay clave)</i>"]
    G2 -->|falla| G3["3. Gemini flash-lite"]
    G3 -->|falla| OFF["4. Coach offline<br/>reglas locales, nunca falla"]

    G1 -->|ok| R["Respuesta"]
    G2 -->|ok| R
    G3 -->|ok| R
    OFF --> R

    classDef seguro fill:#d4f7dc,stroke:#27d17c,color:#000;
    class OFF seguro;
```

Es un **Chain of Responsibility**: cada eslabón intenta y, si falla, cede al siguiente. El último no puede fallar
porque no usa la red. Por eso Koda siempre contesta algo, incluso sin conexión.

El contexto se arma **antes** de preguntar: Koda ve los números reales del usuario y tiene instrucciones explícitas de
no inventar los que no ve.

---

## Inventario de componentes

Lo que un diagrama de treinta cajas no logra comunicar, una tabla sí.

### Web

| Componente | Qué hace |
|---|---|
| `HomeController` | Portada; sin sesión muestra la bienvenida de Koda |
| `AccountController` | Registro, login y logout |
| `PerfilController` | Datos que alimentan los cálculos |
| `AjustesController` | Cuenta, contraseña, zona horaria y equipo |
| `RutinasController` | Rutina y cambio de ejercicios |
| `AlimentacionController` · `DiarioController` | Plan de comidas y adherencia |
| `ProgresoController` · `GamificacionController` | Tracker, niveles y logros |
| `IaCoachController` | Chat y análisis de Koda |
| `UsuariosApi` · `ProgresoApi` · `EntrenamientosApi` | REST bajo `/api/perfil` |

### Application

| Componente | Qué hace |
|---|---|
| `ServicioPerfilUsuario` | Lee y guarda el perfil; lo recuerda por petición |
| `ServicioProgreso` · `ServicioRecords` | Peso y récords personales |
| `ServicioEntrenamientos` | Registra entrenos validando el día real de la rutina |
| `ServicioDiario` | Qué comió el usuario y cómo va contra sus macros |
| `ServicioGamificacion` | Nivel, logros y misiones derivados de los hechos |
| `ServicioSustitucionEjercicios` | Alternativas del mismo grupo muscular |
| `GeneradorRutinasService` · `GeneradorAlimentacionService` | Componen rutina y plan |
| `ZonaHorariaUsuario` | Única fuente de "qué día es para el usuario" |
| `CoachResiliente` · `ArmadorContextoCoach` · `CoachOfflineService` | La cadena de IA |

### Domain

| Componente | Qué hace |
|---|---|
| **Puertos** | `IRepositorioUsuario`, `IRepositorioEjercicios`, `IRepositorioAlimentos`, `IProveedorIA`, `IFabricaProveedoresIA` |
| **Modelos** | `UsuarioPerfil` (+ owned: progreso, entrenamientos, récords, diario), `Ejercicio`, `Alimento`, `Rutina` |
| **Preferencias** | `PreferenciasAlimentarias`, `PreferenciasEntrenamiento` |
| **Cálculo puro** | `CalculadorMacros`, `CalculadorXP`, `CalculadorNivel`, `EvaluadorLogros`, `CalculadorMisiones`, `CalculadorRachas` |
| **Catálogos** | `IndiceEjercicios`, `IndiceAlimentos`, `EquipoEntrenamiento`, `EtiquetasEjercicio` |
| **Strategy** | Una estrategia por objetivo, para rutina y para alimentación |
| **Decorator** | `RutinaConCalentamiento`, `RutinaConEnfriamiento` |

### Infrastructure

| Componente | Qué hace |
|---|---|
| `ApplicationDbContext` | EF Core + Identity |
| `RepositorioUsuarioSql` | Perfil completo con `AsSplitQuery` |
| `RepositorioEjerciciosEnCache` · `RepositorioAlimentosEnCache` | Decorators de caché sobre los repositorios SQL |
| `RepositorioEjerciciosSql` · `RepositorioAlimentosSql` | Acceso real a las tablas |
| Sembradores de catálogo | JSON → SQL al arrancar, solo si la tabla está vacía |
| `GeminiCoachService` · `ProveedorOpenAICompatible` | Adaptadores de IA |
| `FabricaProveedoresIA` | Factory que arma la cadena según la configuración |

---

> **Nota:** `PersonalidadKoda` se llamaba `PersonalidadLoboCoach` hasta que se pagó la deuda **D-32**.
> El coach pasó a llamarse Koda en la Fase 9 y el nombre de la clase se había quedado atrás.
