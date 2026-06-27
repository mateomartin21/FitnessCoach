# ADR-06: Implementación de Patrones GOF y Migración a Arquitectura Multiproyecto

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 26/06/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** Este ADR extiende el ADR-05 (API REST) y formaliza la refactorización de la estructura uniproyecto del ADR-04 hacia una solución multiproyecto. Además documenta la incorporación explícita de tres patrones de diseño GOF al núcleo del sistema.

---

## Contexto

FitnessCoach llegó a la Semana 8 con una arquitectura hexagonal funcional, pero organizada en carpetas dentro de un único `.csproj`. Esta decisión, documentada en el ADR-04, fue correcta para las primeras semanas: redujo el overhead de configuración y permitió avanzar rápido.

Sin embargo, tres factores hacen necesaria una evolución:

- **Frontera débil:** la lógica de negocio (Domain) y la infraestructura compilan juntas, lo que impide garantizar en tiempo de compilación que el dominio no tiene referencias externas prohibidas.
- **Exigencia académica:** la actividad ADR-06 exige implementar y documentar patrones de diseño GOF de forma explícita, no solo como carpetas sino como ciudadanos de primera clase del código.
- **Escalabilidad futura:** escalar el sistema (nuevos Adapters, cliente móvil, base de datos real) requiere que cada capa tenga su propio proyecto con sus propias dependencias declaradas.

Los patrones GOF ya existían parcialmente en el código — Strategy y Decorator fueron introducidos en semanas anteriores — pero no estaban formalizados como una decisión arquitectónica documentada.

---

## Decisión

Se toman dos decisiones simultáneas que se refuerzan mutuamente.

### 1. Migración de uniproyecto a solución multiproyecto

La solución `FitnessCoach.slnx` pasa de un único `.csproj` a cuatro proyectos diferenciados, cada uno con su conjunto de dependencias explícitas:

| Proyecto (.csproj)            | Responsabilidad                                                        | Equivalente Hexagonal   |
|-------------------------------|------------------------------------------------------------------------|-------------------------|
| `FitnessCoach.Domain`         | Modelos, Ports (interfaces), Patrones GOF (Strategy, Decorator)        | Núcleo / Hexágono       |
| `FitnessCoach.Application`    | Servicios: `CalculadorCaloricoService`, `GeneradorRutinaService`       | Casos de Uso            |
| `FitnessCoach.Infrastructure` | `RepositorioUsuarioMemoria` — implementación concreta del Port         | Adapter de salida       |
| `FitnessCoach` (Web)          | Controllers MVC, ApiControllers, Views Razor, wwwroot                  | Adapters de entrada     |

La regla de dependencias es estricta: cada proyecto solo puede referenciar la capa inmediatamente inferior. `FitnessCoach.Domain` no referencia a nadie. Esto convierte la arquitectura hexagonal de una convención de carpetas en una restricción verificada por el compilador.

### 2. Implementación formal de tres Patrones GOF

| Patrón GOF       | Categoría      | Clases involucradas                                                                    | Propósito en FitnessCoach                                                                 |
|------------------|----------------|----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| **Strategy**     | Comportamiento | `IEstrategiaRutina`, `EstrategiaPerderPeso`, `EstrategiaGanarMusculo`, `EstrategiaRecomposicion` | Seleccionar el algoritmo de generación de rutina según el objetivo del usuario en tiempo de ejecución |
| **Decorator**    | Estructural    | `RutinaDecorator`, `RutinaConCalentamiento`, `RutinaConEnfriamiento`                   | Agregar calentamiento y/o enfriamiento a cualquier rutina sin modificar la estrategia base |
| **Factory Method** | Creacional   | `GeneradorRutinaService` → crea `IEstrategiaRutina`                                   | Centralizar la creación de la estrategia correcta según el `ObjetivoFitness` del perfil   |

> *El Factory Method no es un archivo nuevo sino el comportamiento ya presente en `GeneradorRutinaService` al seleccionar la estrategia — se documenta aquí para que quede registrado como decisión de diseño.*

---

## Por qué se eligieron estos patrones

### Strategy — por qué es el patrón correcto para las rutinas

El sistema tiene tres objetivos de fitness y cada uno genera una rutina completamente distinta. Sin Strategy, la selección del algoritmo estaría en un `if/switch` dentro del servicio, creciendo con cada nuevo objetivo. Con Strategy, agregar un cuarto objetivo (por ejemplo, resistencia cardiovascular) es crear una nueva clase `EstrategiaResistencia` sin tocar nada existente — principio Open/Closed.

La interfaz `IEstrategiaRutina` con su único método `GenerarRutina()` es el contrato que permite que el servicio no sepa qué algoritmo está ejecutando, solo que produce una `Rutina`.

### Decorator — por qué es el patrón correcto para calentamiento y enfriamiento

El calentamiento y el enfriamiento son comportamientos opcionales que pueden combinarse libremente con cualquier rutina base. Sin Decorator, habría que crear clases como `EstrategiaPerderPesoConCalentamiento`, `EstrategiaPerderPesoConEnfriamiento`, `EstrategiaPerderPesoConAmbos` — una explosión combinatoria de tres objetivos × tres combinaciones = nueve clases.

Con Decorator, `RutinaConCalentamiento` y `RutinaConEnfriamiento` envuelven cualquier `IEstrategiaRutina` sin importar cuál sea:

```csharp
new RutinaConCalentamiento(new RutinaConEnfriamiento(new EstrategiaPerderPeso()))
```

Esto también explica por qué `RutinaDecorator` implementa `IEstrategiaRutina`: para que un Decorator pueda envolver a otro Decorator, formando una cadena arbitraria.

### Factory Method — por qué centralizar la creación de estrategias

Sin Factory, cada Controller que necesite generar una rutina tendría que saber qué clase concreta instanciar según el objetivo — duplicando la lógica de selección en múltiples lugares. `GeneradorRutinaService` actúa como Factory: recibe un `ObjetivoFitness` y devuelve la `IEstrategiaRutina` correcta. Los Controllers nunca conocen las clases concretas de Strategy.

---

## Diagrama

```
FitnessCoach.slnx
│
├── FitnessCoach.Domain          (sin referencias externas)
│   ├── Models/                   UsuarioPerfil, Rutina, Objetivos...
│   ├── Ports/                    IRepositorioUsuario, IGeneradorRutinas...
│   └── Patterns/
│       ├── Strategy/             IEstrategiaRutina + 3 implementaciones
│       └── Decorator/            RutinaDecorator + Calentamiento + Enfriamiento
│
├── FitnessCoach.Application      → referencia Domain
│   └── Services/                 CalculadorCaloricoService, GeneradorRutinaService
│
├── FitnessCoach.Infrastructure   → referencia Domain
│   └── Repositories/             RepositorioUsuarioMemoria
│
└── FitnessCoach (Web)            → referencia Application + Infrastructure
    ├── Controllers/              MVC: Perfil, Progreso, Rutinas, Home
    ├── Web/ApiControllers/       REST: UsuariosApi, ProgresoApi
    └── Views/                    Razor + wwwroot
```

---

## Gestión de ramas Git

La evolución del proyecto se documenta en tres ramas:

| Rama             | Contenido                                                                                       |
|------------------|-------------------------------------------------------------------------------------------------|
| `mvc-inicial`    | Punto de partida: estructura MVC clásica con Controllers, Models y Services en carpetas planas. Refleja el ADR-02. |
| `master` (hexagonal) | Rama principal. Arquitectura hexagonal multiproyecto con los tres patrones GOF implementados, API REST y Coach IA. |
| `api-layer`      | Rama de extensión. Agrega los ApiControllers REST sobre la base hexagonal. |

Las tres ramas permiten revisar la evolución del sistema desde la arquitectura MVC inicial hasta la hexagonal con patrones GOF y API.

---

## Alternativas Consideradas

### Alternativa 1: Mantener uniproyecto con carpetas

Era la decisión del ADR-04 y sigue siendo válida para proyectos académicos pequeños. Se descarta en este punto porque la actividad exige verificar las fronteras arquitectónicas con referencias de proyecto, no solo con convenciones de carpetas. Un compilador que rechaza referencias prohibidas es más confiable que un acuerdo de equipo.

### Alternativa 2: Template Method en lugar de Strategy

Template Method también permite variar partes de un algoritmo, pero requiere herencia — la subclase sobreescribe pasos del algoritmo de la clase padre. Strategy usa composición — el comportamiento se inyecta. En este dominio los tres tipos de rutina no comparten pasos en común sino que son algoritmos completamente distintos, por lo que la composición es más limpia que la herencia.

### Alternativa 3: Observer para eventos de progreso

Observer permitiría notificar a múltiples componentes cuando el usuario registra un nuevo peso (actualizar racha, verificar logros, recalcular IMC). Se deja como mejora futura documentada. En el estado actual, el volumen de funcionalidades no justifica la complejidad adicional de gestionar suscriptores.

---

## Consecuencias

### ✅ Lo que gana el sistema

- **Fronteras reales:** las fronteras entre capas están verificadas por el compilador — `FitnessCoach.Domain` no puede accidentalmente importar Entity Framework.
- **Open/Closed garantizado:** agregar un nuevo objetivo fitness requiere crear una clase y registrarla en el Factory, sin modificar ninguna clase existente.
- **Composición flexible:** calentamiento y enfriamiento son ortogonales a la estrategia — se activan o desactivan independientemente y en cualquier combinación.
- **Independencia de capas:** Infrastructure puede migrar a PostgreSQL sin tocar Domain ni Application.

### ⚠️ Lo que se asume o sacrifica

- **Complejidad de solución:** cuatro proyectos requieren gestionar referencias y versiones de NuGet por separado. Para un equipo unipersonal académico, es overhead aceptable dado lo que se gana.
- **Factory simple:** el Factory Method en `GeneradorRutinaService` es un `switch` sobre un enum. Si la cantidad de estrategias crece mucho, se podría refactorizar a un `Dictionary<TipoObjetivo, Func<IEstrategiaRutina>>`.
- **Persistencia pendiente:** el repositorio en memoria sigue perdiendo datos al reiniciar. La migración a PostgreSQL se hará creando `RepositorioUsuarioPostgres` en Infrastructure sin tocar Domain — exactamente el beneficio que la arquitectura hexagonal garantiza.

---

## Resiliencia: ¿qué pasa si el servidor falla?

Si FitnessCoach estuviera desplegada en un EC2 en AWS y el servidor se cayera, el sistema quedaría completamente inaccesible — no hay redundancia en la arquitectura actual. Esta sección documenta la respuesta planificada a ese escenario.

### ¿Cómo se detectaría?

- **CloudWatch** monitoreando métricas del EC2 (CPU, memoria, status check).
- **Elastic Load Balancer Health Checks** enviando una petición a `/Health` cada 30 segundos — si el servidor no responde, ELB deja de enviarle tráfico automáticamente (Circuit Breaker a nivel de infraestructura).
- **Alarma de CloudWatch** → notificación por SNS al desarrollador cuando el status check falla por más de 2 minutos consecutivos.

### ¿Cómo se resolvería?

- **Corto plazo (actual):** reinicio manual del EC2 desde la consola de AWS. El tiempo de recuperación estimado es de 3-5 minutos.
- **Mediano plazo:** configurar un **Auto Scaling Group** con mínimo 1 instancia — AWS lanza una nueva instancia automáticamente si la existente falla, sin intervención manual.
- **Largo plazo:** agregar una segunda instancia en una zona de disponibilidad diferente detrás del ELB para eliminar el punto único de falla.

### Impacto en los datos

El repositorio actual (`RepositorioUsuarioMemoria`) pierde todos los datos al reiniciar el servidor. Esto hace que la resiliencia de infraestructura sea incompleta hasta que se migre a PostgreSQL en RDS — que sobrevive reinicios del EC2 al ser un servicio separado.

### Relación con los patrones GOF implementados

El Decorator (`RutinaConCalentamiento`, `RutinaConEnfriamiento`) es el punto de extensión natural para agregar resiliencia a nivel de código: un `RutinaConTimeout` o un `RutinaConFallback` podrían envolver cualquier estrategia existente sin modificarla, devolviendo una rutina por defecto si el servicio externo (por ejemplo, Gemini) no responde.
