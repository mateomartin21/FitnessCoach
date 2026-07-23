# ADR-08: Suite de Pruebas Unitarias (xUnit) y Pipeline de Integración Continua

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 22/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** Este ADR extiende el ADR-07, que dejó formalizada la migración a persistencia real con EF Core y la gestión segura de la API key de Gemini, pero no abordó ningún mecanismo de verificación automática del comportamiento del sistema. Hasta este ADR, la única forma de confirmar que un cambio no rompía nada era ejecutar la aplicación manualmente — exactamente el riesgo que el ADR-06 ya había hecho explícito al documentar los patrones Strategy, Decorator y Factory Method: cualquier cambio en la selección de estrategia o en el orden de los decorators podía romperse en silencio.

---

## Contexto

El ADR-06 formalizó la arquitectura hexagonal multiproyecto con tres patrones GOF (Strategy para rutinas y alimentación, Decorator para calentamiento/enfriamiento e hidratación, Factory Method para reconstruir `ObjetivoFitness`). El ADR-07 resolvió la persistencia real y la seguridad de credenciales externas.

Ninguno de los dos ADRs anteriores dejó constancia de **cómo se verifica que ese comportamiento sigue siendo correcto** después de cada cambio. Esto es deuda técnica implícita: no estaba documentada como tal en el ADR-07 porque hasta ahora no existía ningún mecanismo de prueba automatizada en el proyecto — `FitnessCoach.Tests` no existía.

El riesgo concreto: `GeneradorRutinasService` y `GeneradorAlimentacionService` combinan Strategy + Decorator en cadena (`RutinaConEnfriamiento(RutinaConCalentamiento(estrategia))`). Un error al modificar el orden de esa composición, o un `switch` de Factory al que se le olvida agregar un caso nuevo, no genera ningún error de compilación — solo un comportamiento incorrecto en producción, silencioso hasta que un usuario lo nota.

---

## Decisión

### 1. Proyecto de pruebas aislado del resto de la solución

Se crea `FitnessCoach.Tests` (xUnit) referenciando **únicamente** `FitnessCoach.Domain` y `FitnessCoach.Application` — sin referencia a `FitnessCoach.Infrastructure` ni al proyecto web `FitnessCoach`. Esta decisión es deliberada: las clases con la lógica de negocio más sensible (Strategy, Decorator, Factory, cálculo calórico) no dependen de EF Core, SQL Server ni de la integración con Gemini. Mantener esa frontera significa que la suite corre en segundos, sin necesitar una base de datos ni una API key configurada — condición necesaria para que el pipeline de CI funcione en un runner de GitHub sin secretos ni infraestructura adicional.

Un detalle de configuración no trivial: `FitnessCoach.csproj` (el proyecto web) excluye explícitamente del build las carpetas de los demás proyectos hermanos (`Compile Remove`, `Content Remove`, `EmbeddedResource Remove`) porque comparten la misma carpeta raíz. Al agregar `FitnessCoach.Tests` en esa misma raíz, fue necesario sumar esa carpeta a las tres exclusiones — de lo contrario, `FitnessCoach.csproj` intentaba compilar los archivos de prueba dentro del proyecto web, que no tiene referencia a xUnit.

### 2. Qué se prueba y por qué se eligieron esas clases

| Clase probada | Patrón / Responsabilidad | Por qué se eligió |
|---|---|---|
| `CalculadorCaloricoService` | Fórmula de Mifflin-St Jeor + multiplicador de objetivo | Es la lógica de negocio más sensible a errores de fórmula; un error de signo o de orden de operaciones no lo detecta el compilador |
| `ObjetivoFitnessFactory` | Factory Method | El ADR-07 documenta que esta clase reconstruye el objetivo al leer de la base de datos vía `HasConversion`; si su `switch` queda desincronizado con las clases concretas, el dato persistido deja de poder reconstruirse |
| `RutinaConCalentamiento` / `RutinaConEnfriamiento` | Decorator | Verifican que el decorator inserte/agregue el ejercicio en la posición correcta de **cada** día, no solo del primero — el bug más probable en un `foreach` mal modificado |
| `PlanConHidratacion` | Decorator | Verifica que las recomendaciones de hidratación se agreguen sin alterar el resto del plan generado por la estrategia envuelta |
| `GeneradorRutinasService` / `GeneradorAlimentacionService` | Composición Strategy + Decorator | Prueban que el servicio de aplicación seleccione la estrategia correcta según el tipo de `ObjetivoFitness` y que la aplique envuelta en los decorators correspondientes — es la pieza que conecta ambos patrones y donde un error de `switch` o de orden de wrapping tendría mayor impacto |

Para los tests de Decorator se usaron **test doubles simples** (`EstrategiaFalsa`, `EstrategiaAlimentacionFalsa`) en vez de las estrategias reales (`EstrategiaGanarMusculo`, etc.). Esto aísla la prueba del decorator del contenido real de las rutinas: si mañana se agrega un ejercicio nuevo a `EstrategiaGanarMusculo`, esos tests de decorator no deben romperse — solo deben validar que el decorator hace su trabajo (insertar al inicio, agregar al final), sin importar qué contenga el objeto que envuelve.

En total: **21 pruebas** distribuidas en 4 carpetas (`Services`, `Objetivos`, `Patterns`, `ServicesIntegration`), todas en verde.

### 3. Pipeline de GitHub Actions

`.github/workflows/ci.yml` corre en cada `push` y `pull_request`: restaura, compila la solución completa (`FitnessCoach.slnx`) y ejecuta específicamente `FitnessCoach.Tests.csproj` (no el `.slnx` completo, para no intentar "testear" los otros cuatro proyectos que no tienen pruebas).

**Incidente durante la implementación** (vale la pena dejarlo documentado, ya que el ADR-07 estableció el precedente de registrar hallazgos de esta naturaleza): el primer intento de `ci.yml` no generó ningún check en el Pull Request. La causa real fue doble:
- El archivo se guardó por error en la raíz del repositorio en lugar de `.github/workflows/ci.yml` — GitHub Actions solo reconoce workflows en esa ruta exacta, sin importar que la carpeta se viera correctamente en el Explorador de Soluciones de Visual Studio.
- Adicionalmente, el commit que agrega un workflow por primera vez no dispara una corrida para sí mismo; se necesita un push posterior con el archivo ya existente en la rama.

Una vez movido el archivo a la ruta correcta, el pipeline corrió y quedó en verde tanto en `push` como en el check del Pull Request.

---

## Diagrama de la suite de pruebas
FitnessCoach.Tests
├── Services/
│ └── CalculadorCaloricoServiceTests.cs (3 pruebas)
├── Objetivos/
│ └── ObjetivoFitnessFactoryTests.cs (6 pruebas)
├── Patterns/
│ ├── RutinaDecoratorTests.cs (3 pruebas)
│ └── PlanConHidratacionTests.cs (2 pruebas)
└── ServicesIntegration/
├── GeneradorRutinasServiceTests.cs (4 pruebas)
└── GeneradorAlimentacionServiceTests.cs (3 pruebas)

Referencias: → FitnessCoach.Domain
→ FitnessCoach.Application
(sin Infrastructure, sin proyecto web)
---

## Alternativas Consideradas

### Alternativa 1: NUnit o MSTest en vez de xUnit
Se descarta por continuidad con lo ya definido en el curso (Actividad #35, sobre CitasApp) — las tres alternativas resuelven el mismo problema (`Arrange-Act-Assert`), y cambiar de framework no aporta valor adicional al proyecto.

### Alternativa 2: Usar un framework de mocking (Moq, NSubstitute) en vez de test doubles escritos a mano
Para las pruebas de Decorator se consideró usar `Moq` para simular `IEstrategiaRutina`. Se descarta por ahora: las estrategias falsas escritas a mano (`EstrategiaFalsa`) son suficientemente simples de mantener y no agregan una dependencia adicional al proyecto de pruebas para un caso de uso tan acotado. Si en el futuro se necesitan dobles más complejos (por ejemplo, para probar `GeminiCoachService` sin llamar a la API real), se reevaluará incorporar Moq.

### Alternativa 3: Incluir `FitnessCoach.Infrastructure` en el proyecto de pruebas
Se descarta para esta iteración. Probar `ApplicationDbContext` y `RepositorioUsuarioMemoria` requeriría una base de datos real o en memoria (SQLite in-memory, por ejemplo) y queda fuera del alcance de esta actividad. Se documenta como pendiente.

---

## Consecuencias

### Lo que gana el sistema
- Los tres patrones GOF documentados en el ADR-06 (Strategy, Decorator, Factory Method) ahora tienen verificación automática — un cambio que rompa la composición Strategy + Decorator falla en segundos, no en producción.
- El pipeline de CI corre sin necesitar SQL Server ni la API key de Gemini, gracias a que `FitnessCoach.Tests` no referencia `Infrastructure`.
- Cualquier Pull Request contra la rama base muestra el resultado de las 21 pruebas antes de poder mergear.

### Lo que se asume o sacrifica
- `FitnessCoach.Infrastructure` (persistencia EF Core, integración con Gemini) queda sin cobertura de pruebas automatizadas — sigue dependiendo de verificación manual.
- Los tests de Decorator dependen de test doubles mantenidos a mano; si `IEstrategiaRutina` o `IEstrategiaAlimentacion` cambian de forma, esos dobles también deben actualizarse.
- El pipeline actual es CI puro (verifica, no despliega) — coherente con el alcance definido en la Semana 12 del curso; CD queda como siguiente paso natural, tal como ya lo anticipaba el ADR-07 respecto al despliegue.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Arquitectura hexagonal multiproyecto (ADR-06) y persistencia real (ADR-07) vigentes.
- ✅ Suite de pruebas xUnit: 21 pruebas, cubriendo Strategy, Decorator y Factory Method.
- ✅ Pipeline de GitHub Actions (`ci.yml`) ejecutándose en cada push y Pull Request, con check verde confirmado.
- ⏳ Pendiente: cobertura de pruebas para `FitnessCoach.Infrastructure` (posiblemente con SQLite in-memory para `ApplicationDbContext`).
- ⏳ Pendiente a futuro: extender el pipeline de CI a CD (despliegue automático), como ya se proyectaba en el ADR-07.