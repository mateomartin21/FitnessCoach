# ADR-18: Gamificación derivada de los hechos, sin estado paralelo

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 26/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-12 estableció el tracker como un **historial de hechos** (entrenamientos, peso, récords) con las reglas en la capa de aplicación. Este ADR construye la gamificación **enteramente encima de esos hechos**: el nivel, el XP, los logros y las misiones no son un estado de juego que se guarda y actualiza, sino una **lectura derivada** de lo que el usuario ya hizo. Es la Fase 8 del roadmap.

> **Nota de numeración:** el roadmap asignaba ADR-17 a esta fase, pero la Fase 7 (pulido de IA) necesitó su propio ADR-17. Como ya pasó con el ADR-11, los ADR siguientes se corren un número: gamificación es **ADR-18**.

---

## Contexto

La Fase 8 pide las mecánicas de juego que sostienen la constancia (05-VISION-PRODUCTO): niveles/experiencia, logros desbloqueables, misiones semanales, y el Lobo reaccionando. La investigación de producto dejó una advertencia clara (§77): *lo que sostiene a largo plazo es el progreso visible y que el esfuerzo se sienta significativo, no acumular insignias por acumular.*

La decisión de fondo era **cómo modelar el estado de juego**: ¿se guarda (tablas de XP, logros desbloqueados, misiones) y se actualiza en cada acción, o se **deriva** de los hechos ya registrados?

---

## Decisión

### 1. Todo se deriva de los hechos, no se guarda estado de juego

No hay tablas nuevas ni migración. El XP, el nivel, los logros y las misiones se **calculan** cada vez a partir de los hechos que el tracker ya guarda: entrenamientos, récords, historial de peso y diario. La pieza central es `EstadisticasUsuario`, una foto plana de esos hechos que arma la capa de aplicación (`ServicioGamificacion`), y que los calculadores de dominio leen.

**Por qué:** un estado de juego persistido puede desincronizarse de la realidad (borro un entrenamiento y el XP no baja; un bug deja un logro marcado sin haberlo logrado). Derivándolo, el estado de juego **siempre** refleja los hechos, por construcción. Además queda como lógica de dominio pura, ideal para xUnit —el *Definition of Done* de la fase—, sin base de datos de por medio.

### 2. Los calculadores viven en el dominio y son funciones puras

En `Domain/Models/Gamificacion`:
- `CalculadorXP` — traduce los hechos a XP. La constancia paga más: hay un bono por la mejor racha además del XP por cada acción (§77).
- `CalculadorNivel` — convierte XP en nivel con una curva creciente de RPG (subir cuesta cada vez más) y títulos de lobo (Cachorro → Lobo Alfa).
- `Logro` + `CatalogoLogros` + `EvaluadorLogros` — 12 logros anclados a hechos reales, cada uno con un criterio **medible** (no solo sí/no), así se puede mostrar el progreso hacia el que falta.
- `Mision` + `CalculadorMisiones` — 3 misiones medidas sobre los últimos 7 días, que se "reinician" solas al pasar la ventana.

Todos reciben `EstadisticasUsuario` y devuelven un resultado; ninguno toca infraestructura ni reloj propio.

### 3. Cada logro trae su reacción del Lobo

La personalidad es del producto (D-20, ADR-16): por eso cada logro carga la línea con que el Lobo lo festeja, y viajan juntos. Al registrar un entrenamiento o un récord, el controlador compara la foto de antes con la de después (`EvaluadorLogros.ReciénDesbloqueados`) y, si algo se cruzó, muestra el aviso en la voz del Lobo en el momento.

### 4. La pantalla como barra de progreso, no como vitrina de badges

`GamificacionController` solo lee: no hay nada que escribir. La vista es de estilo RPG (barra de XP, nivel, misiones y logros con su progreso), coherente con la visión, y pone al frente el **avance** —cuánto falta para el próximo nivel, cuánto para el próximo logro— antes que la colección de insignias.

---

## Alternativas Consideradas

### Alternativa 1: Persistir el estado de juego (tablas de XP, logros, misiones)
El enfoque clásico. **Se descarta:** agrega migración y superficie de bugs de sincronización, y contradice el criterio del ADR-12 (los hechos son la fuente de verdad). El costo de recalcular es trivial —son unas cuentas sobre listas que ya están en memoria con el perfil—.

### Alternativa 2: Otorgar XP como evento en el momento de cada acción
Sumar XP a un contador al registrar cada hecho. **Se descarta por el mismo motivo:** el contador podría divergir de los hechos (borrados, correcciones). Derivar el XP total garantiza que siempre cuadre con lo que el usuario hizo.

### Alternativa 3: Muchos logros y misiones para "dar volumen"
Una lista larga engancha en la demo. **Se descarta:** §77 es explícito en que el volumen vacío no sostiene. Se eligió un set acotado y significativo (12 logros, 3 misiones), todos atados a progreso real.

### Alternativa 4: Que el Lobo (IA) narre cada logro con una llamada al modelo
Reusar la IA de la Fase 6. **Se descarta para el aviso inmediato:** gastaría una llamada (y cuota del tier gratuito) en cada logro, y el aviso debe ser instantáneo y funcionar sin conexión. Las líneas de los logros son fijas y en personaje; la IA ya narra lo dinámico (resumen semanal, Fase 7).

---

## Consecuencias

### Lo que gana el sistema
- **El estado de juego nunca miente:** siempre refleja los hechos, porque se deriva de ellos. Sin migración ni sincronización.
- **Lógica de dominio pura y cubierta:** XP, nivel, logros y misiones son funciones testeables; +26 pruebas en la fase.
- **Constancia recompensada** por sobre la acumulación vacía (§77): bono por racha, logros de progreso, misiones semanales accionables.
- **El Lobo reacciona** al desbloquear un logro, en su voz, al instante y sin depender de la IA.
- 323 pruebas en verde.

### Lo que se asume o queda pendiente
- **Recálculo en cada carga:** se recomputan las estadísticas cada vez. Es barato hoy (listas en memoria); si el historial creciera mucho, habría que cachear. No es un problema a esta escala.
- **La ventana semanal y las rachas se cuentan en la hora del servidor** (D-25, Fase 10): un usuario en otra zona horaria puede ver el corte de "esta semana" desalineado con su medianoche.
- **La curva de XP y el set de logros son un primer balance**, no un ajuste fino de *game design*. Están pensados para tocarse fácil: agregar un logro es una línea en el catálogo.
- **El aviso de logro usa TempData:** se muestra una vez tras la acción. No hay un historial de "cuándo desbloqueaste cada logro" (haría falta persistir la fecha del hecho que lo gatilló); la pantalla muestra el estado actual, no la línea de tiempo.
- **El autorreporte es inherente, pero se cerró el "escribir cualquier cosa":** como los logros de entrenamiento se ganan al marcar un entrenamiento hecho, y ese registro era texto libre, cualquiera podía anotar algo inventado y llevarse el XP. Se ató el registro a los **días reales de la rutina** del usuario (desplegable + validación en el servidor). No verifica que la persona haya entrenado de verdad —ningún tracker autorreportado puede—, pero elimina el input arbitrario.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ `EstadisticasUsuario` + calculadores de dominio (XP, nivel, logros, misiones), todos puros.
- ✅ `ServicioGamificacion` arma el resumen desde el perfil, sin persistencia propia.
- ✅ Pantalla de logros estilo RPG y entrada en el menú.
- ✅ Aviso del Lobo al desbloquear un logro tras entrenar o marcar un récord.
- ✅ `dotnet build` sin errores; 323/323 pruebas en verde.
- ⏳ Sigue la Fase 9 (rediseño pixel art y Lobo Coach), que ahora tiene mecánicas que vestir.
