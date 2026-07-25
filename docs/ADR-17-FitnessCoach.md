# ADR-17: El Lobo en toda la app y el resumen semanal narrado

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 25/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-16 dejó a la IA convertida en una capa sobre el sistema, con contexto real, anclada al catálogo y con un endpoint `Analizar` que ya aceptaba distintos aspectos (progreso, dieta, rutina). Este ADR **cierra la Fase 7** aprovechando esa base: lleva el análisis del Lobo a las pantallas de dieta y rutina —sin código nuevo de IA— y agrega lo único genuinamente nuevo, el **resumen semanal narrado**.

---

## Contexto

Tras el ADR-16, el Lobo ya analizaba con datos reales, pero solo desde la pantalla de Progreso. El endpoint `Analizar` estaba preparado para `aspecto = dieta` y `aspecto = rutina`, y `PedidoDeAnalisis` ya devolvía el pedido correspondiente, pero **ninguna pantalla los invocaba**. Era trabajo hecho a medias: la capacidad existía y no se ofrecía.

Además, la Fase 7 tenía una pieza que el ADR-16 no absorbió: un **resumen semanal** en la voz del Lobo (cuántas veces entrenó, cómo viene la racha, cómo se movió el peso). Ese dato no estaba en el contexto —el armador incluía el peso reciente y los récords, pero no el pulso de los últimos 7 días—.

---

## Decisión

### 1. Una sola tarjeta de análisis, reutilizable

La tarjeta *"El Lobo analiza…"* que vivía embebida en la vista de Progreso (marcado + `fetch` + token) se extrajo a un **partial** (`Views/Shared/_AnalisisLobo.cshtml`) parametrizado por el aspecto. Las pantallas de Progreso, Alimentación y Rutina la incluyen con una línea (`<partial name="_AnalisisLobo" model="@("dieta")" />`), cada una con su título, su texto y su llamada al endpoint ya existente.

El partial mantiene la regla de seguridad del ADR-16: la respuesta de la IA se inserta con `textContent`, nunca `innerHTML`. Los ids llevan el aspecto como sufijo para que dos tarjetas en una misma pantalla no colisionen.

No hubo que tocar el controlador ni la personalidad: el `Analizar` con `aspecto` y los pedidos de dieta/rutina ya estaban listos desde la fase anterior. Esto es puramente **exponer** una capacidad latente.

### 2. El pulso de la semana en el contexto

`ArmadorContextoCoach` suma un bloque `== ESTA SEMANA (ultimos 7 dias) ==`: entrenamientos hechos en la ventana, racha actual y mejor racha (con el mismo `CalculadorRachas` de la pantalla de progreso), y la variación de peso de la semana. Como todo el armador, va protegido: si no hay actividad ni pesos de la semana, el bloque se omite.

El bloque no es solo para el resumen: al vivir en el contexto, **cualquier** respuesta del Lobo conoce ahora el ritmo reciente de la persona.

### 3. El resumen semanal narrado

Se agregó el aspecto `semana` a `PedidoDeAnalisis`: le pide al Lobo que narre la semana con los datos del bloque nuevo, reconozca lo bien hecho y marque una cosa concreta para la siguiente. Se ofrece como una tarjeta más (reusando el partial) arriba del análisis de progreso.

---

## Alternativas Consideradas

### Alternativa 1: Un resumen semanal calculado, sin IA
Un texto armado con plantillas ("Entrenaste 3 veces, +0,4 kg"). **Se descarta:** la gracia de la Fase 7 es la *voz* del Lobo. El dato duro ya está en las tarjetas de Progreso; el resumen aporta el tono y el consejo, que es justo lo que la IA hace bien.

### Alternativa 2: Duplicar la tarjeta y su script en cada vista
Copiar el bloque de Progreso a Alimentación y Rutina. **Se descarta:** tres copias del mismo `fetch` y el mismo manejo de token envejecen mal. El partial deja una sola fuente de verdad.

### Alternativa 3: Un job que calcule el resumen semanal en segundo plano
Precalcular y guardar el resumen. **Se descarta por sobrediseño:** mantiene el criterio del ADR-16 de análisis **bajo demanda**, sin gastar cuota del tier gratuito ni sumar infraestructura de tareas programadas para un proyecto académico.

---

## Consecuencias

### Lo que gana el sistema
- **El Lobo acompaña en toda la app:** analiza la dieta desde el plan y la rutina desde la pantalla de entrenamiento, no solo el progreso.
- **Resumen semanal en personaje**, con entrenamientos, racha y peso reales.
- **Menos duplicación:** una sola tarjeta de análisis para las cuatro variantes.
- **Contexto más rico:** el bloque semanal mejora también las respuestas del chat.
- 297 pruebas en verde (+3): el bloque semanal del armador y el nuevo pedido `semana`.

### Lo que se asume o queda pendiente
- **Sigue siendo bajo demanda:** el resumen no se manda solo ni por correo; se pide con un botón. Un resumen empujado (notificación semanal) queda para cuando exista la Fase 8 (gamificación) con su sistema de eventos.
- **Las mismas limitaciones de anclaje del ADR-16:** los ejercicios se anclan a la rutina, no al catálogo completo; la calidad del texto depende del proveedor.
- **El bloque semanal cuenta en la hora del servidor** (como las rachas), pendiente de la zona horaria del usuario (D-25, Fase 10).

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Partial `_AnalisisLobo` reutilizable; análisis del Lobo en Progreso, Alimentación y Rutina.
- ✅ Bloque `ESTA SEMANA` en el contexto y pedido de análisis `semana` con su tarjeta.
- ✅ `dotnet build` sin warnings nuevos; 297/297 pruebas en verde.
- ✅ **Fase 7 cerrada.** La línea de IA (Fases 6 y 7) queda completa.
- ⏳ Sigue la Fase 8 (gamificación): logros, niveles, misiones y el Lobo reaccionando a los eventos.
