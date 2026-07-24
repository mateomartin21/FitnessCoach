# 05 — Visión de producto

> **La frase que define todo:** *"quiero que la temática se vea como un videojuego pixel art donde el Lobo Coach sea el que te guíe y te diga funcionalidades, que se vea con personalidad y no como un proyecto genérico."*
>
> Este documento existe para que ninguna decisión técnica contradiga esa idea. Cuando haya que elegir entre dos implementaciones igual de correctas, gana la que sirva a esta visión.

---

## El concepto

FitnessCoach no es un formulario que devuelve una rutina. Es **un juego donde el personaje eres tú, y el Lobo Coach es el NPC que te guía**.

La diferencia práctica entre las dos cosas:

| Enfoque genérico | Enfoque FitnessCoach |
|------------------|----------------------|
| "Perfil guardado correctamente." | El Lobo aparece y te dice: *"Ya te tengo fichado. Vamos a trabajar."* |
| Una tabla con tu historial de peso | Tu progreso como barra de experiencia, con el Lobo comentándolo |
| Un chatbot en una pestaña aparte | El Lobo está presente en toda la app, reaccionando a lo que haces |
| "Rutina generada" | *"Te armé esta rutina. 4 días. No me falles."* |
| Error 500 | El Lobo se encoge de hombros: *"Se me fue la señal, campeón. Dame un segundo."* |

**El Lobo Coach no es una feature. Es la interfaz.**

---

## Los cuatro pilares

### 1. Identidad visual: pixel art

- Existe un **modelo del lobo en pixel art** que define el estilo de toda la app.
- La paleta, la tipografía y los componentes se derivan de ese personaje — no al revés.
- Referencias de estilo: UI de RPG clásico (cajas de diálogo, barras de HP/XP, inventario por celdas), no "app fitness moderna con degradados".
- **Animaciones y transiciones** son parte del estilo, no un adorno: el lobo debe tener estados (idle, celebrando, decepcionado, pensando) y las pantallas deben transicionar como en un juego.

### 2. El Lobo como guía omnipresente

- Aparece en las pantallas clave, no solo en el chat de IA.
- **Anuncia y explica funcionalidades** en vez de que el usuario tenga que descubrirlas leyendo menús.
- Reacciona al contexto real: si el usuario lleva 5 días sin registrar peso, lo dice. Si rompió un récord, lo celebra.
- Su voz es consistente: motivador, directo, cercano — la que ya está definida en el prompt actual de Gemini.

### 3. Progresión con mecánicas de juego

El usuario debe **ver que avanza**, no solo tener datos guardados:
- Rachas (días consecutivos entrenando)
- Récords personales (PRs) con notificación
- Niveles / experiencia por constancia
- Logros desbloqueables
- Misiones semanales ("3 entrenamientos esta semana")

### 4. Contenido real, no de muestra

Una app así solo se sostiene si el contenido está a la altura:
- Catálogo amplio de ejercicios, no 3 por rutina
- **GIFs demostrativos**, sobre todo pensados para principiantes que no saben ejecutar el movimiento
- Variedad suficiente para que dos usuarios con el mismo objetivo no vean exactamente lo mismo

---

## Referencias: qué hacen bien las apps reales

Investigación de apps de gimnasio y fitness gamificado vigentes en 2026, y qué tomar de cada una:

| App | Qué hace bien | Qué tomar |
|-----|--------------|-----------|
| **Hevy** | El estándar en registro rápido de sets/reps/peso. Plantillas reutilizables, temporizador de descanso, calculadora de discos. Su fuerza es la **fricción mínima al registrar** | El modelo de logging de la Fase 4: registrar una serie debe tomar 2 toques, no un formulario |
| **Jefit** | Biblioteca de +1,400 ejercicios **con demostraciones**, sobrecarga progresiva guiada, gráficas de progresión de 1RM, dashboard de récords personales, seguimiento de rachas | El catálogo con GIFs (Fase 5) y el dashboard de PRs (Fase 4) |
| **RazFit** | Combina coaching adaptativo por IA con un sistema completo de 32 logros | Es la referencia más cercana a la visión: **IA + gamificación juntas**, no como módulos separados |
| **Habitica** | Convierte tareas reales en un RPG completo: avatar, equipo, misiones, daño por incumplir | La estética y las mecánicas RPG (Fase 8) |
| **Zombies, Run!** | Narrativa que convierte el ejercicio en una historia; el audio te mete en un mundo | La idea de que **un personaje narrando cambia por completo la experiencia** — exactamente el rol del Lobo |
| **Ring Fit Adventure** | Mascota/personaje que guía, corrige y celebra durante el ejercicio | El tono del Lobo como acompañante activo |
| **Strava / Peloton** | Capa social: tablas de clasificación, retos, comunidad | ⚠ Fuera de alcance por ahora — requiere multiusuario maduro. Anotado como idea futura |

### La lección de fondo de esa investigación

Las rachas y las tablas de clasificación enganchan durante el primer mes. Lo que sostiene a largo plazo es el **progreso visible** y que el esfuerzo se sienta significativo. Traducido a decisiones para FitnessCoach: es más valioso invertir en que el usuario **vea su evolución** (gráficas, PRs, el Lobo reconociéndolo) que en acumular badges por acumular.

---

## Ideas propias (para evaluar, no comprometidas)

Extras que encajan con la visión y no están en el roadmap todavía:

- **Estados de ánimo del Lobo según constancia** — si abandonas una semana, el sprite cambia; vuelve a la normalidad cuando retomas. Refuerzo emocional barato de implementar y muy alineado al concepto.
- **"Ficha de personaje"** — la pantalla de perfil como una hoja de personaje de RPG: estadísticas (fuerza, resistencia, constancia) derivadas de datos reales del entrenamiento.
- **El plan de alimentación como "inventario"** — las comidas presentadas como ítems consumibles con sus valores.
- **Análisis semanal narrado por el Lobo** — la IA lee tu historial real y te da un resumen en su voz. Une la Fase 4 (datos) con la Fase 7 (IA) de forma natural.
- **Sonidos de 8 bits** en las acciones clave (guardar, completar, récord). Enorme retorno en personalidad por muy poco esfuerzo técnico.
- **Modo oscuro como "noche"** en la estética del juego, no como un simple toggle.

---

## Restricciones de la visión

Estas se documentan para que no se olviden al entusiasmarse:

1. **La arquitectura no se sacrifica por la estética.** El pixel art vive en `wwwroot` y en las vistas. `Domain` no sabe que existe un lobo.
2. **El rediseño visual va al final del roadmap**, deliberadamente. Re-skinnear pantallas ya estables es barato; rediseñar pantallas que todavía cambian por debajo es trabajo tirado a la basura.
3. **Sigue siendo un proyecto académico evaluado.** La personalidad no puede costar puntos en arquitectura, pruebas o documentación — y este es precisamente el argumento a favor de mantener el estándar de `03-ESTANDARES.md`: la app puede verse como un juego *y* estar bien construida por dentro.
