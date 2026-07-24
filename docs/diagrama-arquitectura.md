# Diagramas de arquitectura C4 — FitnessCoach

Estos diagramas reflejan el estado real del código de esta rama.

## Nivel 1 — Contexto del sistema

```mermaid
C4Context
    title Nivel 1 - Contexto del sistema (FitnessCoach)

    Person(usuario, "Usuario", "Persona que configura su perfil, genera rutinas y registra su progreso de peso")

    System(fitnessCoach, "FitnessCoach", "Plataforma web de coaching fitness (ASP.NET Core MVC + API, .NET 9/10)")

    Rel(usuario, fitnessCoach, "Usa", "HTTPS")
```

## Nivel 2 — Contenedores

```mermaid
C4Container
    title Nivel 2 - Contenedores (FitnessCoach)

    Person(usuario, "Usuario", "Persona que usa la plataforma")

    System_Boundary(fitnessCoach, "FitnessCoach") {
        Container(app, "FitnessCoach (ASP.NET Core)", "MVC + Web API en un mismo proceso", "Sirve las vistas (Perfil, Rutinas, Progreso) y expone /api/perfil y /api/perfil/progreso. Program.cs actua como composition root")
        ContainerDb(memoria, "Almacenamiento en memoria", "RepositorioUsuarioMemoria", "Guarda los usuarios y su progreso en memoria; no hay base de datos persistente todavia")
    }

    Rel(usuario, app, "Usa", "HTTPS")
    Rel(app, memoria, "Lee/escribe")
```

## Nivel 3 — Componentes

```mermaid
flowchart TB
    subgraph WEB["FitnessCoach (ASP.NET Core - MVC + API, un solo proceso)"]
        direction LR
        Program["Program.cs<br/>(Composition Root)"]
        PerfilCtrl["PerfilController"]
        RutinasCtrl["RutinasController"]
        ProgresoCtrl["ProgresoController"]
        IaCoachCtrl["IaCoachController<br/>(vista vacia, sin logica aun)"]
        UsuariosApi["UsuariosApiController"]
        ProgresoApi["ProgresoApiController"]
    end

    subgraph APP["FitnessCoach.Application"]
        direction LR
        CalculadorSvc["CalculadorCaloricoService"]
        GeneradorSvc["GeneradorRutinasService"]
    end

    subgraph DOMAIN["FitnessCoach.Domain — nucleo"]
        direction LR
        IRepo["IRepositorioUsuario"]
        ICalculador["ICalculadorCalorico"]
        IGenerador["IGeneradorRutinas"]
        IEstrategia["IEstrategiaRutina"]
        Models["Models: UsuarioPerfil, ObjetivoFitness,<br/>Rutina, RegistroProgreso"]
    end

    subgraph PATTERNS["FitnessCoach.Domain.Patterns"]
        direction LR
        Estrategias["EstrategiaPerderPeso / EstrategiaGanarMusculo /<br/>EstrategiaRecomposicion<br/>(Strategy — una por objetivo)"]
        Calentamiento["RutinaConCalentamiento<br/>(Decorator)"]
        Enfriamiento["RutinaConEnfriamiento<br/>(Decorator)"]
    end

    subgraph INFRA["FitnessCoach.Infrastructure"]
        RepoMemoria["RepositorioUsuarioMemoria<br/>(almacenamiento en memoria, sin BD)"]
    end

    PerfilCtrl --> IRepo
    PerfilCtrl --> ICalculador
    RutinasCtrl --> IRepo
    RutinasCtrl --> IGenerador
    ProgresoCtrl --> IRepo
    UsuariosApi --> IRepo
    UsuariosApi --> ICalculador
    ProgresoApi --> IRepo

    Program -.compone.-> RepoMemoria
    Program -.compone.-> CalculadorSvc
    Program -.compone.-> GeneradorSvc

    CalculadorSvc --> ICalculador
    CalculadorSvc --> Models

    GeneradorSvc --> IGenerador
    GeneradorSvc -->|"1 - selecciona segun ObjetivoFitness"| Estrategias
    Estrategias -->|"2 - envuelta por"| Calentamiento
    Calentamiento -->|"3 - envuelta por"| Enfriamiento
    Enfriamiento -->|"4 - implementa"| IEstrategia

    RepoMemoria --> IRepo

    classDef patron fill:#fff3cd,stroke:#333;
    class Estrategias,Calentamiento,Enfriamiento patron;
```
