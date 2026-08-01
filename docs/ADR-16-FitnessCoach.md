# ADR-16: El Lobo Coach resiliente, con contexto real y anclado al catálogo

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 25/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-13 ensayó una cadena de respaldos para los medios de los ejercicios (GIF → video → búsqueda → placeholder). Este ADR aplica el mismo patrón a la IA del Lobo Coach y lo lleva más lejos: además de no morirse cuando un proveedor falla, la IA pasa a **ver los datos reales** del usuario (plan, rutina, diario, récords) y a **anclar** sus recomendaciones al catálogo, sin inventar. Cierra **D-09** (errores devueltos como consejos) y **D-20** (personalidad dentro del adaptador).

> **Nota de alcance:** esta fase se amplió respecto del roadmap original. La Fase 6 iba a ser solo resiliencia de IA; a pedido del usuario absorbió el grueso de la Fase 7 (IA expandida): contexto rico, análisis sobre datos reales y la IA como capa sobre el sistema, no un chat aislado.

---

## Contexto

La IA del Lobo tenía tres problemas de fondo:

- **D-09:** ante un fallo, `GeminiCoachService` devolvía el texto del error (`"Error de conexion..."`) por el mismo canal que una respuesta válida, y el `catch` era mudo. El llamador no podía distinguir éxito de fallo, lo que **bloqueaba cualquier fallback** y dejaba los errores sin registrar.
- **D-20:** el prompt que define la personalidad del Lobo vivía incrustado en el adaptador HTTP. Cambiar de proveedor obligaba a reescribir la personalidad, y viceversa.
- **Era un chat a ciegas.** El "contexto" que recibía la IA eran cuatro datos del perfil (nombre, edad, peso, objetivo). No veía el plan de comidas, la rutina, el diario ni los récords, así que sus respuestas eran genéricas. Y como no conocía el catálogo, podía recomendar alimentos o ejercicios que no existen en la app.

El usuario pidió explícitamente: que la IA vea "prácticamente todo el perfil", que sus recomendaciones salgan de "los alimentos y ejercicios que están en nuestra base de datos, para que no se invente nada", que no quede en "un simple chat", y que el fallback use **un Factory** que cambie a otra IA si Gemini está caído.

---

## Decisión

### 1. Un puerto de proveedor, y errores que son excepciones

`IProveedorIA` (Domain/Ports) es un puerto genérico: recibe una `ConsultaIA` y devuelve texto, o **lanza `CoachIAException`**. Que el fallo sea una excepción y no un string es lo que hace posible todo lo demás: la cadena puede distinguir un fallo de una respuesta buena y pasar al siguiente proveedor (D-09). Cada fallo se registra con `ILogger` —antes no se registraba nada—.

El controlador dejó de depender de `GeminiCoachService` (una clase de Infraestructura, violación de la regla de dependencias) y ahora depende de `ICoachIA`, el caso de uso en Application.

### 2. La personalidad del Lobo vive en Application, no en el adaptador

`PersonalidadLoboCoach` (Application) arma el prompt y guarda la respuesta de "sin señal". El adaptador de Gemini solo se ocupa de "cómo hablo con Google": recibe el prompt ya armado (D-20). Cambiar de proveedor no toca la personalidad, ni al revés.

De paso se le dio **más carácter** al Lobo (entrenador de la vieja escuela que conoce a su pupilo) y **reglas que no puede romper**: solo recomendar lo que esté en el contexto, nunca inventar, y responder con los datos reales de la persona.

### 3. Chain of Responsibility + Factory

`CoachResiliente` (Application) arma el prompt una vez y prueba los proveedores **en orden**: usa el primero que contesta, registra el que falla y sigue. Si todos caen, el Lobo responde igual con su frase de "sin señal" —nunca un error crudo—. Una respuesta vacía se trata como fallo, para no mostrar un globo en blanco.

Quién produce esa lista es un **Factory** (`IFabricaProveedoresIA`), como pidió el usuario: qué proveedores existen y con qué modelos/claves se decide en un solo lugar, desde la configuración. Agregar un proveedor nuevo es tocar solo la fábrica.

### 4. Resiliencia real y gratuita, por capas

El orden de la cadena está pensado para degradar con gracia sin costar dinero:

1. **Gemini** (modelo principal) — el primario, con la clave gratuita que ya se usaba.
2. **Groq / OpenRouter**, si hay clave — respaldo de **otra empresa**, que sobrevive a una caída de Google. Ambos tienen tier gratuito y hablan el protocolo de OpenAI, así que **un solo adaptador** (`ProveedorOpenAICompatible`) cubre los dos. Se activan solos al configurar la clave.
3. **Gemini** (modelo secundario) — reintento barato ante un problema puntual del primer modelo.
4. **Offline por reglas** (`CoachOfflineService`) — la última garantía: sin red, responde por reglas según el tema de la pregunta, en la voz del Lobo, y **nunca falla**.

Con esto la meta de la fase se cumple sin pagar nada: con internet cortado, los proveedores de red fallan y el offline responde igual; la app no se rompe y el personaje sigue en pie.

### 5. Contexto rico anclado al catálogo

`ArmadorContextoCoach` (Application) junta todo lo que el sistema sabe del usuario —perfil, preferencias, peso reciente, récords, plan de comidas, diario de hoy y rutina— más la **lista real de alimentos del catálogo**. Cada bloque se arma protegido: si uno falla (perfil sin datos válidos para el plan), se omite ese bloque y el resto igual se arma.

Esa lista de alimentos reales, sumada a la regla del prompt ("solo puedes recomendar lo que esté acá, nunca inventes"), es lo que **ancla** a la IA a la base de datos: no puede sugerir un alimento que la app no tiene.

### 6. La IA como capa, no un chat

Un endpoint `Analizar` corre el mismo contexto rico con un pedido según el aspecto (progreso, dieta o rutina) y devuelve el análisis del Lobo con los datos reales de la persona. La pantalla de Progreso suma una tarjeta *"El Lobo analiza tu progreso"* que lo pide **bajo demanda** —para no gastar una llamada a la IA en cada carga— y muestra la respuesta con `textContent`, nunca `innerHTML`.

---

## Alternativas Consideradas

### Alternativa 1: La IA genera la rutina y la dieta desde cero
Máximo protagonismo de la IA. **Se descarta:** todo lo construido en las fases 5–5.6 es un sistema determinista, testeable, offline y siempre válido. Si la IA genera el plan, puede inventar un ejercicio inexistente o macros que no cierran, y sin internet no habría plan. Se eligió la **IA como capa**: el catálogo + Strategy arma el plan válido; la IA lo lee, lo comenta y sugiere ajustes anclados a lo que existe.

### Alternativa 2: Un segundo proveedor pago (Anthropic Haiku, OpenAI)
Modelos de primera línea. **Se descarta por costo:** ni Anthropic ni OpenAI tienen API gratuita, y el proyecto no puede pagar por uso. Los respaldos elegidos (Groq, OpenRouter) son de otras empresas y tienen tier gratuito, con el mismo protocolo de OpenAI para no escribir dos adaptadores.

### Alternativa 3: Meter todo el catálogo de ejercicios en el contexto
Anclar también los ejercicios como se ancla la comida. **Se descarta:** son 1.323 ejercicios; incluirlos en cada prompt es inviable en tokens. El contexto ancla los alimentos (67, baratos) por completo, y para ejercicios se apoya en la rutina del usuario, que ya sale del catálogo. Es la limitación registrada más abajo.

### Alternativa 4: Devolver la respuesta degradada como un error HTTP
Que el front distinga "IA caída". **Se descarta:** la visión quiere que el usuario vea al personaje encogiéndose de hombros, no un error. La cadena siempre devuelve `200` con la respuesta del Lobo; el flag `EsDegradada` permite un matiz en la UI sin romper la experiencia.

### Alternativa 5: Análisis automático al cargar cada pantalla
Más "mágico". **Se descarta:** gastaría una llamada a la IA (y cuota del tier gratuito) en cada visita. El análisis es bajo demanda, con un botón.

---

## Consecuencias

### Lo que gana el sistema
- **La IA no se muere.** Con Gemini caído o sin internet, la cadena degrada a otro proveedor o al offline; la app nunca muestra un error crudo. Cierra **D-09**.
- **La personalidad es del producto, no del adaptador.** Cambiar de proveedor no la toca. Cierra **D-20**.
- **El Lobo ve los datos reales** (plan, rutina, diario, récords) y responde con ellos, no con generalidades.
- **No inventa.** Sus recomendaciones se anclan al catálogo real de la app.
- **Es una capa, no un chat:** analiza el progreso desde la pantalla con los números concretos.
- **Fallback gratuito y de otra empresa** disponible con una clave gratis (Groq/OpenRouter), armado con un **Factory** extensible.
- 294 pruebas en verde (+30 en la fase): cadena, respaldo offline, armador de contexto y personalidad.

### Lo que se asume o queda pendiente
- **El respaldo de otra empresa exige una clave gratuita.** Hasta que se configure `Groq:ApiKey` (o `OpenRouter:ApiKey`), la cadena es Gemini principal → Gemini secundario → offline: cubre un modelo saturado y la caída total (offline), pero no una caída de Google con respuesta inteligente. Es configurar una clave gratis, sin tocar código.
- **Los ejercicios se anclan a la rutina, no al catálogo completo** (1.323, inviable en tokens). La IA puede referirse a los ejercicios de la rutina del usuario; para explorar más, está el catálogo navegable de la app.
- **El respaldo offline es genérico**, no personalizado: da consejos correctos y en personaje según el tema, pero no lee los datos del usuario. Su rol es que "siempre haya algo", no reemplazar a la IA.
- **Las pruebas no cubren los adaptadores de red** (Gemini, OpenAI-compatible, la fábrica): `FitnessCoach.Tests` no referencia Infraestructura (ADR-08). La resiliencia se prueba con proveedores falsos; el camino real queda para la prueba de fuego (internet desconectado).
- **La calidad del anclaje depende del proveedor.** El prompt prohíbe inventar, pero un modelo puede desobedecer; el sistema reduce el riesgo (le da solo datos reales), no lo elimina.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ `IProveedorIA` + `CoachIAException`: los proveedores lanzan al fallar, la cadena distingue y registra.
- ✅ Personalidad del Lobo en Application, con más carácter y reglas de no-invención.
- ✅ `CoachResiliente` (Chain of Responsibility) + `IFabricaProveedoresIA` (Factory).
- ✅ Gemini (dos modelos) + adaptador Groq/OpenRouter (gratis, por config) + offline como última garantía.
- ✅ `ArmadorContextoCoach`: contexto rico anclado al catálogo real.
- ✅ Análisis del Lobo sobre datos reales, desde la pantalla de Progreso.
- ✅ `dotnet build` sin warnings; 294/294 pruebas en verde.
- ⏳ Pendiente: configurar una clave gratuita de Groq/OpenRouter para el respaldo de otra empresa (acción del usuario, sin código).
- ⏳ La Fase 7 original queda casi absorbida; lo que resta es pulido (comentarios del Lobo en más pantallas, resumen semanal narrado).
