# ADR-19: Identidad visual pixel art y Koda, el coach, presente y con vida

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 26/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-16 dio al coach una personalidad de producto (el Lobo) y una IA resiliente; el ADR-17 lo llevó a toda la app; el ADR-18 le dio mecánicas (niveles, logros) que reaccionan a los hechos. Este ADR es la **capa visual** que envuelve todo eso: el coach pasa a llamarse **Koda**, la app adopta un lenguaje **pixel art** y Koda aparece —y reacciona— en las pantallas clave, no solo en el chat. Es la Fase 9 del roadmap.

> **Nota de numeración:** el roadmap adelantaba "ADR-18" para esta fase, pero ese número lo tomó la gamificación. La identidad visual es **ADR-19**.

---

## Contexto

La Fase 9 pide la transformación visual completa (05-VISION-PRODUCTO): que ninguna pantalla parezca una plantilla de Bootstrap y que la app se reconozca como propia en una sola captura. Se dejó al final a propósito: re-skinnear pantallas ya estables es barato; rediseñar pantallas que todavía cambian por debajo es trabajo perdido.

Había además tres decisiones de fondo que no eran solo "pintar":
1. **Identidad del coach.** El "Lobo Coach" era un nombre genérico. El producto necesitaba un nombre con personalidad.
2. **De dónde salen los assets.** El diseño del personaje (el lobo) y los logos no se generan con IA; los aporta el usuario. La app tenía que estar lista para recibirlos sin rehacer nada.
3. **Idioma.** Toda la redacción estaba en español rioplatense (voseo). El usuario es de México: el español correcto para el producto usa "tú", no "vos".

---

## Decisión

### 1. Koda, el coach, y una paleta azul

El coach se llama **Koda**. La app sigue siendo **FitnessCoach**, con Koda como su entrenador. El color primario pasó del naranja a un **azul** tomado del character sheet, y se cablearon los **6 colores de estado** del sheet a su contexto real: entrenar (azul), logro (verde), recuperar (naranja), descanso (rojo), análisis (púrpura), recomendación (turquesa). Son tokens CSS (`--fc-*`) reutilizados en toda la app, no colores sueltos por vista.

### 2. Un sistema de diseño pixel/8-bit, no un tema más de Bootstrap

En `wwwroot/css/site.css`, como variables de diseño: bordes duros (sin blur), sombras sólidas, esquinas rectas (`border-radius: 0`), `image-rendering: pixelated`, barras de XP segmentadas, y la tipografía **Press Start 2P** (auto-hospedada, licencia OFL, sin CDN externo) reservada para títulos y etiquetas de marca. Se descartó el filtro de *scanlines* retro: ensuciaba la lectura sin aportar identidad.

### 3. Koda presente y reactivo, con una capa de JavaScript vanilla

Koda deja de vivir solo en el chat. Un único sprite sheet que aporta el usuario se recorta por *bounding-box* del canal alfa en 19 sprites (una figura hero, 5 caras, 6 poses, 6 tarjetas de estado) y se cablean a sus pantallas (hero de Inicio, chat, Rutina, Progreso, Logros, tarjeta de análisis). Encima, dos módulos JS sin librerías (`wwwroot/js/`):

- **`koda.js`** — Koda *reactivo* (cambia de expresión: piensa mientras el chat responde, sonríe al contestar, se sorprende ante un error, celebra al entrar a Logros), *micro-interacciones* (respiración idle, rebote al click con Web Animations API y `composite:'add'`, para sumarse a la animación idle sin pelearse por el `transform`) y un *aura de partículas* en canvas (píxeles neón azules ceñidos a la caja del sprite).
- **`logros.js`** — cada logro se dibuja como una **medalla pixel en canvas** (moneda octagonal con bisel, en el color de su categoría, con un glifo) que **reemplaza a los emojis**. Desbloqueada brilla y late; bloqueada va en gris. Sin imágenes que descargar.

**Por qué canvas y no imágenes:** las medallas y el aura son consistentes con la paleta por construcción, se recolorean con un token, y no dependen de que existan PNGs. Los sprites del propio Koda sí son imágenes, porque son arte del personaje que aporta el usuario.

### 4. Localización a español de México

Todo el texto visible pasó de voseo a tuteo (vistas, controladores, respaldo offline, catálogo de logros y misiones, descargo médico). La personalidad de la IA (`PersonalidadLoboCoach`) suma una regla explícita en el prompt: *responde siempre en español de México, usa "tú", nunca "vos"*, para que Koda no vuelva a hablar rioplatense. Se actualizaron los asserts de pruebas que verificaban esas cadenas.

---

## Alternativas Consideradas

### Alternativa 1: Un tema CSS oscuro "bonito" sobre Bootstrap
Cambiar colores y tipografía sin comprometerse con el pixel art. **Se descarta:** el *Definition of Done* de la fase es que la app **no** parezca una plantilla. Medio camino habría dado justo eso.

### Alternativa 2: Generar los sprites del lobo y los logos con IA
Rápido y sin depender de terceros. **Se descarta por decisión del usuario:** el arte del personaje lo aporta él (lo busca/descarga). La app solo prepara los espacios y los recorta; no inventa el personaje.

### Alternativa 3: Íconos de logros con emojis o con Font Awesome enmarcado
Lo más barato. **Se descarta:** los emojis se ven ajenos al pixel art y varían por sistema operativo; un ícono de fuente en un marco no es lo mismo que una medalla dibujada. El canvas da una pieza propia, on-brand y recoloreable.

### Alternativa 4: Animar los sprites cuadro a cuadro (walk cycles, parpadeo)
La animación pixel "de verdad". **Se descarta por los assets:** cada estado es **un solo cuadro**. Sin *frames* no hay animación cuadro a cuadro; se logra vida con transform/opacidad (idle, rebote), swaps de estado y el aura de partículas, que no necesitan más cuadros.

---

## Consecuencias

### Lo que gana el sistema
- **Identidad propia:** ninguna pantalla se lee como Bootstrap crudo. Paleta, tipografía, bordes y componentes son un sistema, no parches por vista.
- **Koda es un personaje, no un avatar quieto:** reacciona en el chat, celebra logros, respira y responde al click, con su aura neón.
- **Logros con pieza gráfica propia:** medallas de canvas por categoría, sin descargar nada y recoloreables por token.
- **Español correcto para el usuario** en toda la app y en la voz de la IA.
- **Handoff de arte limpio:** el contrato de nombres (`wwwroot/images/koda/README.txt`) permite soltar sprites nuevos y que la app los tome sin tocar código.
- `dotnet build` sin errores; 323/323 pruebas en verde.

### Lo que se asume o queda pendiente
- **El recorte de fondo del sprite sheet dejó un halo blanco tenue** en un par de poses. Se **disimula** con el glow neón y el flote (decisión del usuario), no se eliminó píxel a píxel para no arriesgar el pelaje blanco del lobo. Si molesta, se limpia el borde por defringe.
- **Sprites de un solo cuadro:** no hay animación cuadro a cuadro; la vida es por CSS/JS y partículas. Sumar frames es un trabajo de arte futuro.
- **Las medallas de logros y el aura dependen de JavaScript.** Sin JS, el `<canvas>` cae al emoji de reserva; el aura simplemente no aparece. Todo respeta `prefers-reduced-motion` (sin flote, rebote ni partículas; glow fijo).
- **Sonidos 8-bit (entregable opcional de la fase): no se hicieron.** Quedan fuera de alcance por ahora.
- **Los PNG viejos `wwwroot/images/branding/*` quedaron sin uso** tras cablear los sprites de Koda; se pueden borrar en una limpieza.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Sistema de diseño pixel/8-bit en tokens CSS; Press Start 2P auto-hospedada; sin scanlines.
- ✅ Coach renombrado a **Koda**; primario azul; 6 colores de estado cableados.
- ✅ 19 sprites recortados del sheet y cableados a las pantallas clave.
- ✅ `koda.js`: Koda reactivo, micro-interacciones y aura de partículas (respeta reduced-motion).
- ✅ `logros.js`: medallas pixel dibujadas en canvas, reemplazando los emojis.
- ✅ App localizada a español de México, incluida la voz de la IA.
- ✅ `dotnet build` sin errores; 323/323 pruebas en verde.
- ⏳ Sigue la Fase 10 (optimización, despliegue y cierre).
