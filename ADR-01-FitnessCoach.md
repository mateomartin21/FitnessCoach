# ADR-02: Arquitectura inicial de FitnessCoach — App web de seguimiento nutricional y fitness

| Campo  | Valor |
|--------|-------|
| Autor  | Mateo Martin |
| Fecha  | 17/05/2025 |
| Estado | `Propuesto` |

---
Este ADR sustituye el ADR01 entregado en la practica anterior, con mejor explicacion de la decision de arquitectura.
## Contexto

Estoy construyendo **FitnessCoach**, una aplicación web que funciona como un coach de nutrición y fitness personalizado. El problema que resuelve es que muchas personas no tienen acceso a un nutriólogo o entrenador personal, y las apps genéricas no llevan un seguimiento adaptado a sus objetivos individuales.

Los usuarios podrán registrar sus datos personales (peso, estatura, edad), definir objetivos (bajar de peso, ganar músculo, mantener), recibir recomendaciones de alimentación y rutinas, y ver su progreso a lo largo del tiempo.

**Condiciones y restricciones que influyen en esta decisión:**

- El proyecto se desarrolla en ASP.NET Core con C#, tecnología que el equipo (yo) está aprendiendo durante el cuatrimestre.
- Es un proyecto individual, sin equipo distribuido.
- Los temas vistos en clase cubren MVC, MVVM, MVP, Arquitectura Hexagonal y principios SOLID.
- La plataforma de destino es web (navegador), no móvil ni escritorio.

---

## Decisión

Elijo el **patrón arquitectónico MVC (Model-View-Controller)** implementado con **ASP.NET Core** como framework, aplicando los **principios SOLID** como guía de diseño interno de las clases.

### ¿Por qué MVC?

MVC separa tres responsabilidades claramente diferenciadas que se alinean con exactamente lo que mi app necesita:

- **Model** → Los datos del usuario: su perfil, sus objetivos, su progreso diario (peso, calorías, etc.).
- **View** → Las pantallas que el usuario ve: el dashboard con su progreso, el formulario de registro de comidas, la tabla de rutinas.
- **Controller** → La lógica que coordina: recibir la petición del usuario, consultar o guardar datos en el Model, y devolver la View correcta.

La razón concreta por la que MVC resuelve mi problema es la **separación de responsabilidades**: si mañana quiero cambiar cómo se ve el dashboard, solo modifico la View, sin tocar la lógica de negocio. Si cambio cómo se calcula el índice calórico, solo toco el Model o un servicio, sin romper las pantallas. Esto es especialmente valioso porque estoy aprendiendo y voy a cometer errores — MVC me permite corregirlos en un lugar sin afectar el resto del sistema.


### Principios SOLID aplicados

Los principios SOLID no son la arquitectura, son las reglas de diseño interno de mis clases. Los aplico así:

**S — Single Responsibility:** Cada clase tiene una sola razón para cambiar. No voy a tener una clase `UsuarioManager` que calcule calorías, guarde datos Y envíe correos. Tendré clases separadas: `PerfilUsuario` (datos), `CalculadorCalorico` (lógica), `RegistroProgreso` (seguimiento).

**O — Open/Closed:** Si en el futuro agrego nuevos tipos de objetivos fitness (por ejemplo, "mejorar resistencia" además de "bajar de peso" y "ganar músculo"), no voy a modificar la clase base de objetivos — la voy a extender. Usaré una clase abstracta `ObjetivoFitness` de la que heredan `ObjetivoPerderPeso`, `ObjetivoGanarMusculo`, etc.

**D — Dependency Inversion:** Mi Controller no va a hacer `new RepositorioUsuario()` directamente. Va a depender de una interfaz `IRepositorioUsuario`, lo que me permite empezar con datos en memoria y después conectar una base de datos sin cambiar la lógica de negocio.

Los principios L e I los consideraré al diseñar jerarquías de clases más adelante, evitando subclases que rompan contratos (LSP) e interfaces demasiado grandes (ISP).

### Alternativas consideradas

| Alternativa | Por qué la descarté |
|-------------|---------------------|
| **MVVM** | Está diseñado para apps de escritorio o móviles (WPF, MAUI, Angular) donde la UI reacciona a cambios de estado en tiempo real con data binding. Mi app es web tradicional con peticiones HTTP — no necesito sincronización automática entre View y ViewModel. |
| **MVP** | Es útil cuando la View debe ser completamente pasiva y necesitas probar la lógica de presentación sin instanciar una UI real. Para una app web en ASP.NET Core, el propio framework ya gestiona ese aislamiento mediante la separación de Controllers y Views. MVP brillaría más en Android nativo. |
| **Arquitectura Hexagonal** | Es la opción más robusta para sistemas empresariales con alta necesidad de testabilidad y donde la lógica de negocio debe ser completamente independiente de la infraestructura. Sin embargo, requiere definir Ports y Adapters desde el inicio, lo que añade una capa de abstracción que va más allá de lo visto en clase hasta ahora y excede el tiempo disponible para este proyecto. Podría migrarse a este patrón si el proyecto crece. |

---

## Consecuencias

**✅ Lo que gano:**

- **Consecuencia técnica:** La separación Model-View-Controller hace que agregar nuevas funcionalidades (por ejemplo, un módulo de rutinas de ejercicio además del de nutrición) sea aditivo, no destructivo. Agrego un nuevo Controller y sus Views sin tocar lo que ya funciona. Esto también me permite encontrar errores más rápido porque sé exactamente en qué capa buscar: ¿el dato está mal? → Model. ¿La pantalla se ve rara? → View. ¿La petición no llega bien? → Controller.

- **Consecuencia sobre el proceso:** Trabajando solo, MVC me da una estructura clara de qué construir primero y qué después. Puedo tener el Controller y el Model funcionando y probarlo antes de preocuparme por el diseño visual de las Views. ASP.NET Core ya genera la estructura de carpetas automáticamente, lo que reduce el tiempo de configuración inicial.

**⚠️ Lo que sacrifico o asumo:**

- **Limitación técnica:** MVC en ASP.NET Core renderiza las páginas en el servidor (server-side rendering). Esto significa que cada vez que el usuario registra una comida o actualiza su peso, habrá una petición HTTP completa al servidor y la página se recargará. No tendré la experiencia "en tiempo real". Para una primera versión esto es aceptable, pero si quiero mostrar gráficas que se actualicen sin recargar la página, necesitaré incorporar JavaScript o migrar partes a una API REST en el futuro.

- **Deuda técnica:** Actualmente los datos estarán en memoria (igual que en la práctica del catálogo). Cuando conecte una base de datos real, necesitaré implementar correctamente las interfaces `IRepositorio*` que defino desde ahora (aplicando DIP). Si no las defino bien desde el inicio, la migración a base de datos obligará a modificar los Controllers, lo que va en contra del principio OCP.

---

## Diagrama

El siguiente diagrama muestra la estructura de nivel 1 del sistema:

```
graph TD
    User1[Usuario <br> Navegador] -- HTTP GET /dashboard --> Controller1
    
    subgraph Servidor [ASP.NET Core MVC]
        Controller1[<b>CONTROLLER</b> <br> Recibe petición]
        Model[<b>MODEL + SERVICIOS</b> <br> UsuarioPerfil, RegistroComida, <br> IRepositorioUsuario]
        Controller2[<b>CONTROLLER</b> <br> Pasa el Model a la View]
        View[<b>VIEW .cshtml</b> <br> Dashboard.cshtml, Perfil.cshtml]
        
        Controller1 -- Consulta / Guarda --> Model
        Model -- Devuelve datos --> Controller2
        Controller2 -- Orquesta --> View
    end
    
    View -- HTML renderizado --> User2[Usuario <br> Navegador]
    
    style Controller1 fill:#d1e7dd,stroke:#0f5132,stroke-width:2px
    style Controller2 fill:#d1e7dd,stroke:#0f5132,stroke-width:2px
    style Model fill:#fff3cd,stroke:#664d03,stroke-width:2px
    style View fill:#cff4fc,stroke:#055160,stroke-width:2px
```
