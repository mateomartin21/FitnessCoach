# Índice de contexto — FitnessCoach

> **Para qué sirve esta carpeta:** estos documentos son la memoria del proyecto. Están escritos para que cualquier persona (o cualquier asistente de IA) que no haya visto nunca FitnessCoach pueda entender en 10 minutos qué es, cómo está construido, qué está roto, qué falta y en qué orden hacerlo — sin tener que leer los ~90 archivos de código.

**Ubicación esperada en el repo:** `docs/contexto/`

---

## Los documentos

| # | Archivo | Qué contiene | Cada cuánto cambia |
|---|---------|--------------|--------------------|
| 01 | `01-PROYECTO.md` | Qué es la app, stack, versiones, estructura de la solución, cómo compilar/correr/probar, ramas | Rara vez |
| 02 | `02-ARQUITECTURA.md` | Reglas de arquitectura hexagonal, qué puede depender de qué, dónde va cada cosa, patrones GOF en uso | Rara vez |
| 03 | `03-ESTANDARES.md` | El estándar de calidad: seguridad, validación, pruebas, nombres, commits, *definition of done* | Rara vez |
| 04 | `04-DEUDA-TECNICA.md` | Inventario real de bugs y deuda detectados, con severidad y estado | **Constantemente** |
| 05 | `05-VISION-PRODUCTO.md` | La visión: pixel art, Lobo Coach, gamificación, referencias de apps reales | Ocasionalmente |
| 06 | `06-ROADMAP.md` | El plan por fases, con dependencias, entregables y criterios de cierre | **Al cerrar cada fase** |

---

## Cómo usar esto en una sesión nueva con IA

**Arranque de sesión — pega esto:**

> Estoy trabajando en FitnessCoach. Te adjunto los documentos de contexto (`docs/contexto/`). Voy a trabajar en la **Fase N** del roadmap. Léelos antes de proponerme nada y respeta `02-ARQUITECTURA.md` y `03-ESTANDARES.md` en todo lo que generes.

Y adjunta:
1. Los 6 documentos de esta carpeta (siempre).
2. El ADR más reciente (contexto de la última decisión formal).
3. Un `.zip` de la rama actual, **o** los archivos concretos que se van a tocar en esa fase.

**Al cerrar una sesión productiva**, pide: *"actualízame `04-DEUDA-TECNICA.md` y `06-ROADMAP.md` con lo que hicimos hoy"*. Si no se actualizan, el contexto envejece y deja de servir.

---

## Reglas de mantenimiento

1. **Estos documentos no reemplazan a los ADRs.** Un ADR registra *una decisión tomada en un momento dado* y no se edita después (es histórico). Estos documentos describen *el estado actual* y sí se editan. Cuando una fase toma una decisión de arquitectura relevante → sale un ADR nuevo, y además se actualiza el estado aquí.
2. **`04-DEUDA-TECNICA.md` es el más importante de mantener al día.** Es el único lugar donde vive la lista honesta de lo que está mal. Si se arregla algo, se marca como resuelto con la fase que lo resolvió; no se borra la línea (el historial sirve).
3. **Nada se documenta aquí como "hecho" si no está verificado.** El proyecto ya tuvo un caso de esto: el ADR-07 daba la persistencia por resuelta, pero `Program.cs` seguía registrando el repositorio en memoria. Antes de marcar algo como hecho: compila, corre, y las pruebas pasan.

---

## Estado del set

| Campo | Valor |
|-------|-------|
| Creado | 22/07/2026 |
| Última actualización | 30/07/2026 |
| Rama de referencia | `fase-10/optimizacion` (contra `CD/CI`) |
| Último ADR | ADR-21 (entrada sin sesión, centro de ajustes y el equipo del usuario como filtro de la rutina) |
| Fase activa del roadmap | Ninguna — Fase 12 cerrada (ADR-21): ajustes, bienvenida, equipo del usuario y sustitución de ejercicios |
