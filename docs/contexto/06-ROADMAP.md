# 06 — Roadmap por fases

> **Cómo leer esto:** las fases están ordenadas por **dependencia técnica**, no por entusiasmo. Cada una desbloquea la siguiente. Saltarse el orden significa rehacer trabajo.
>
> **Regla de oro:** una fase no se cierra hasta cumplir su *Definition of Done* y la checklist general de `03-ESTANDARES.md` §6.

---

## Mapa de dependencias

```
Fase 0  Saneamiento
   │     (base limpia — no depende de nada)
   ▼
Fase 1  Persistencia real  ◄── desbloquea TODO lo demás
   │     (sin esto, nada de lo que el usuario haga sobrevive)
   ▼
Fase 2  Identity + multiusuario + blindaje
   │     (sin esto, "mi progreso" no significa nada)
   ▼
Fase 3  Validación y robustez de dominio
   │
   ├──────────────┬──────────────────┐
   ▼              ▼                  ▼
Fase 4        Fase 5             Fase 6
Tracker       Catálogo de        Resiliencia
              ejercicios         de IA
   │              │                  │
   │              │                  ▼
   │              │              Fase 7
   │              │              IA expandida
   │              │                  │
   └──────────────┴──────────────────┘
                  ▼
             Fase 8  Gamificación
                  ▼
             Fase 9  Rediseño pixel art + Lobo Coach
                  ▼
             Fase 10 Optimización y cierre
```

---

## Fase 0 — Saneamiento

**Objetivo:** dejar la base limpia antes de construir encima. Todo lo de aquí es barato, rápido, y evita arrastrar ruido durante 10 fases.

**Resuelve:** D-08, D-13, D-14, D-15, D-16, D-17, D-18, D-19

**Entregables:**
- Actualizar `Microsoft.AspNetCore.OpenApi` para eliminar el warning `NU1903`
- Convertir todos los `.cs` a UTF-8 y reparar los acentos corruptos
- Corregir el namespace `Repositoriess` → `Repositories` (y su uso en `Program.cs`)
- Renombrar `CalculadorCaloricoServicecs.cs`, `GeneradorRutinaService.cs`, `IGeneradorRutina.cs`
- Borrar los tres `Class1.cs`
- Mover `ErrorViewModel` de `Domain` al proyecto web
- Reparar el markdown escapado de los ADR-07

**Definition of Done:** `dotnet build` sin ningún warning; `dotnet test` en verde (21/21); los ADR renderizan bien en GitHub.
**ADR:** no requiere (no hay decisión de arquitectura, solo higiene).
**Rama sugerida:** `fase-0/saneamiento`

---

## Fase 1 — Persistencia real

> **✅ Cerrada el 23/07/2026** (commit `b85dae1`, ADR-09) — `RepositorioUsuarioSql` conectado como `Scoped`, datos verificados tras reiniciar el servidor. D-03 y D-06 resueltas.

**Objetivo:** que los datos sobrevivan a un reinicio. Es el pendiente que el ADR-07 dejó explícitamente abierto.

**Resuelve:** D-03, D-06

**Entregables:**
- `RepositorioUsuarioSql` en `Infrastructure/Repositories/`, implementando `IRepositorioUsuario` sobre `ApplicationDbContext`
- Reemplazar el registro en `Program.cs`: `AddSingleton<...Memoria>` → `AddScoped<...Sql>`
- Decidir el destino de `RepositorioUsuarioMemoria`: conservarlo (útil para pruebas) o eliminarlo. **Documentar la decisión.**
- Verificar que la migración `InitialCreate` está aplicada y las tablas responden

**Definition of Done:** crear un perfil, reiniciar el servidor, y que el perfil siga ahí. Registrar progreso, reiniciar, y que el historial siga ahí.
**ADR:** ADR-09 — cierre formal de la deuda de persistencia del ADR-07.
**Rama sugerida:** `fase-1/persistencia-sql`

> ⚠ **Ojo:** el `HasConversion` del `ObjetivoActual` (Factory Method) recién se ejercita de verdad en esta fase. Es el primer momento donde se comprueba que el Strategy sobrevive al viaje de ida y vuelta a la base de datos.

---

## Fase 2 — Identity, multiusuario y blindaje

> **✅ Cerrada el 24/07/2026** (rama `fase-2/identity-login`, ADR-10) — seis commits: Identity cableado (`bb8b0f9`), login/registro/logout (`3a078b6`), resolución del perfil por identidad (`1dc0e52`), blindaje de la API (`926ae0c`), pruebas del servicio (`398dc85`) y documentación. D-01, D-02, D-05, D-07 y D-11 resueltas. 34/34 pruebas en verde.
>
> Deuda nueva detectada durante la fase: **D-21** (CSRF en `IaCoachController`) y **D-22** (login sin bloqueo por intentos fallidos), ambas planificadas para la Fase 3.

**Objetivo:** que cada quien tenga su cuenta y solo pueda tocar lo suyo. Es la fase más grande y la más crítica en seguridad.

**Resuelve:** D-01, D-02, D-05, D-07, D-11

**Entregables:**
- Paquete `Microsoft.AspNetCore.Identity.EntityFrameworkCore` en `Infrastructure`
- `ApplicationUser : IdentityUser` en `Infrastructure/Identity/`
- `ApplicationDbContext` hereda de `IdentityDbContext<ApplicationUser>`
- `UsuarioPerfil.IdentityUserId` (`string?`) — **el dominio no conoce Identity**, guarda un identificador opaco
- Índice único sobre `IdentityUserId` (invariante "un usuario = un perfil" garantizada por la BD)
- Migración de EF Core
- `UseAuthentication()` + `UseAuthorization()` en el orden correcto en `Program.cs`
- `AccountController` con Registro / Login / Logout, **con vistas propias** (no el scaffolding por defecto — debe verse como el resto de la app)
- `[Authorize]` en todos los controladores MVC y API
- Eliminar todo `ObtenerPorId(1)`; resolver el perfil desde la identidad autenticada
- `[ValidateAntiForgeryToken]` en todas las acciones POST
- Mensajes de login genéricos (sin enumeración de usuarios)
- Pruebas de la lógica de resolución de perfil por usuario

**Definition of Done:** la prueba de fuego completa de `03-ESTANDARES.md` §7, puntos 1, 2 y 4. Dos usuarios distintos, datos completamente aislados, y ningún endpoint accesible sin sesión.
**ADR:** ADR-10 — autenticación con ASP.NET Identity manteniendo el dominio libre de framework.
**Rama sugerida:** `fase-2/identity-login`

---

## Fase 3 — Validación y robustez

> **✅ Cerrada el 24/07/2026** (rama `fase-3/validacion`, ADR-11) — seis commits: anotaciones (`565965d`), `ModelState` y ViewModels (`5e842c9`), guardas del cálculo y `RangosPerfil` (`c9e0114`), CSRF del chat y bloqueo de cuenta (`d5c1670`), rate limiter por IP (`78183fb`), pruebas de casos límite (`eb6b7fa`) y documentación. D-04, D-21 y D-22 resueltas. 66/66 pruebas en verde y **cero deuda crítica abierta**.
>
> Se sumó un ADR que el plan no preveía: la fase tomó dos decisiones que contradicen reglas escritas (validar dentro del dominio y revelar el bloqueo de cuenta), y eso no podía quedar solo en comentarios del código.
>
> Deuda nueva: **D-23** (la vista de Progreso no existe, detectada al empezar la fase → Fase 4) y **D-24** (rate limiter en memoria y sin cabeceras de proxy → Fase 10).

**Objetivo:** que sea imposible meter datos basura, por la vía que sea.

**Resuelve:** D-04, D-21, D-22

**Entregables:**
- Anotaciones de validación en `UsuarioPerfil` y `RegistroProgreso` con los rangos de `03-ESTANDARES.md` §1.2
- Token antiforgery en el `fetch()` de `IaCoachController.Consultar` (D-21), único POST que quedó sin cubrir en la Fase 2
- `lockoutOnFailure: true` + configuración de `options.Lockout` y política de contraseñas más estricta (D-22)
- `ModelState.IsValid` verificado en **todos** los controladores que reciben datos
- Refactor de `PerfilController.GuardarPerfil`: recibir un modelo, no 5 parámetros sueltos
- Mensajes de error visibles en las vistas (`asp-validation-for`)
- Guardas de dominio donde una anotación no alcanza (ej. `CalcularCaloriasDiarias` con estatura o peso inválidos)
- Pruebas de cada caso límite

**Definition of Done:** prueba de fuego §7, punto 3. Ningún dato inválido llega a la base por ninguna ruta (vista o API).
**ADR:** ADR-11 — validación en dos capas y defensa en profundidad del login (no estaba previsto; ver la nota de cierre).
**Rama sugerida:** `fase-3/validacion`

---

## Fase 4 — Tracker de progreso

**Objetivo:** convertir el historial de peso en un tracker de verdad, al estilo Hevy/Jefit.

**Resuelve:** D-10, D-12, D-23

**Entregables:**
- **`Views/Progreso/Index.cshtml`, que hoy no existe (D-23):** el controlador ya devuelve `View(historial)` contra una vista ausente, así que la pantalla revienta. Es la base sobre la que se monta todo lo demás de esta fase.
- `Id` real para `RegistroProgreso` en el dominio (permite editar/borrar registros individuales)
- Todas las fechas en UTC; conversión solo al mostrar
- Registro de **entrenamientos completados**, no solo de peso
- Cálculo de rachas (días consecutivos)
- Récords personales por ejercicio
- Gráfica de evolución del peso
- Vista de historial con edición y borrado

**Definition of Done:** un usuario puede registrar un entrenamiento, verlo en su historial, ver su racha actual y su gráfica de peso. Todo cubierto por pruebas.
**ADR:** ADR-12 si el modelo de dominio cambia de forma significativa.
**Rama sugerida:** `fase-4/tracker`

---

## Fase 5 — Catálogo de ejercicios y variedad

**Objetivo:** contenido real. Que dos usuarios con el mismo objetivo no vean exactamente lo mismo.

**Entregables:**
- Entidad `Ejercicio` como concepto de primera clase (hoy es una clase suelta dentro de las estrategias), con: nombre, grupo muscular, nivel, equipo necesario, URL de GIF, instrucciones
- Catálogo persistido en base de datos (con datos semilla), no hardcodeado dentro de cada Strategy
- Las estrategias pasan a **componer rutinas desde el catálogo** en vez de tener los ejercicios incrustados
- GIFs demostrativos, con énfasis en principiantes
- Suficiente variedad para rotación

**Definition of Done:** agregar un ejercicio nuevo no requiere tocar ninguna clase de Strategy. Los tests del Decorator siguen pasando sin cambios (por eso usan estrategias falsas).
**ADR:** ADR-13 — el catálogo desacoplado de las estrategias es un cambio de diseño relevante.
**Rama sugerida:** `fase-5/catalogo-ejercicios`

> ⚠ Es la fase con más impacto sobre las pruebas existentes. Los tests de `GeneradorRutinasService` que hoy verifican `rutina.Nivel` pueden necesitar ajuste — revisar antes de empezar.

---

## Fase 6 — Resiliencia de IA

**Objetivo:** que si Gemini falla, el Lobo Coach no muera. Encaja perfecto con la línea de patrones GOF del proyecto.

**Resuelve:** D-09, D-20

**Entregables:**
- Puerto `ICoachIA` en `Domain/Ports/` — hoy los controladores dependen de la clase concreta `GeminiCoachService`, lo cual es una violación de la regla de dependencias
- `GeminiCoachService` implementa ese puerto
- Al menos un proveedor alternativo (otro modelo, u otro proveedor)
- **Fallback:** Strategy + Factory, o Chain of Responsibility, para pasar al siguiente proveedor ante un fallo
- Respuesta degradada garantizada si todos fallan (el Lobo responde algo con personalidad, nunca un error crudo)
- Los errores se propagan de verdad (excepción o tipo resultado), no como texto de respuesta
- El prompt de personalidad del Lobo se extrae del adaptador HTTP
- Registro de fallos con `ILogger`
- Pruebas del fallback con proveedores falsos

**Definition of Done:** prueba de fuego §7, punto 6 — con internet desconectado, el Lobo responde con gracia y la app no se cae.
**ADR:** ADR-14 — resiliencia de IA mediante patrón de proveedores intercambiables.
**Rama sugerida:** `fase-6/resiliencia-ia`

---

## Fase 7 — IA expandida

**Objetivo:** que la IA deje de ser solo un chat y participe en la experiencia.

**Depende de:** Fase 4 (necesita datos que analizar) y Fase 6 (necesita ser confiable antes de ponerla en más lugares).

**Entregables:**
- Análisis de progreso: la IA lee el historial real y comenta la evolución
- Resumen semanal narrado en la voz del Lobo
- Recomendaciones de ajuste basadas en datos reales
- Comentarios contextuales del Lobo en las pantallas clave

**Definition of Done:** el análisis usa datos reales del usuario (no genéricos) y degrada con gracia si la IA no está disponible.
**ADR:** ADR-15 si el diseño de la integración cambia sustancialmente.
**Rama sugerida:** `fase-7/ia-analisis`

---

## Fase 8 — Gamificación

**Objetivo:** las mecánicas de juego que sostienen la constancia.

**Depende de:** Fase 4 (las mecánicas se alimentan de los datos del tracker).

**Entregables:**
- Sistema de logros
- Niveles / experiencia por constancia
- Misiones semanales
- Notificaciones de récord y de racha
- El Lobo reacciona a cada uno de estos eventos

**Definition of Done:** las mecánicas se calculan desde datos reales, y están cubiertas por pruebas (es lógica de dominio pura, ideal para xUnit).
**ADR:** ADR-16.
**Rama sugerida:** `fase-8/gamificacion`

---

## Fase 9 — Rediseño pixel art y Lobo Coach

**Objetivo:** la transformación visual completa. Ver `05-VISION-PRODUCTO.md`.

**Va al final a propósito:** re-skinnear pantallas ya estables es barato; rediseñar pantallas que aún cambian por debajo es trabajo perdido.

**Entregables:**
- Sistema de diseño derivado del modelo pixel art del lobo (paleta, tipografía, componentes)
- Rediseño de todas las vistas
- Sprites del Lobo con estados (idle, celebrando, decepcionado, pensando)
- Animaciones y transiciones entre pantallas
- El Lobo presente y reactivo en las pantallas clave, no solo en el chat
- Sonidos 8-bit en acciones clave *(opcional)*

**Definition of Done:** ninguna pantalla parece una plantilla de Bootstrap. La app se reconoce como propia en una captura.
**ADR:** ADR-17 — decisión de identidad visual y su implementación.
**Rama sugerida:** `fase-9/pixel-art`

---

## Fase 10 — Optimización y cierre

**Objetivo:** el pulido final.

**Entregables:**
- Rate limiter listo para producción (D-24): almacén compartido en vez de memoria del proceso, y `UseForwardedHeaders` para no contar todo el tráfico bajo la IP del balanceador
- Revisión de consultas de EF Core (detectar N+1, agregar `AsNoTracking` donde aplique)
- Índices de base de datos según los patrones de consulta reales
- Caché donde tenga sentido (catálogo de ejercicios)
- Optimización de estáticos (imágenes, GIFs)
- Revisión de accesibilidad
- README actualizado
- Los 7 puntos de la prueba de fuego, completos
- Actualizar `04-DEUDA-TECNICA.md` y este roadmap

**Definition of Done:** las siete pruebas de fuego pasan; cero deuda crítica o alta abierta.
**Rama sugerida:** `fase-10/optimizacion`

---

## Ideas fuera de alcance (registradas, no comprometidas)

- Capa social (retos entre amigos, tablas de clasificación) — requiere multiusuario maduro
- App móvil / PWA
- Despliegue continuo (CD) a EC2 — extensión natural del pipeline actual
- Migración a PostgreSQL / RDS — ya anticipada en el ADR-06 y el ADR-07
- Integración con wearables

---

## Bitácora de fases

| Fase | Estado | Rama | PR | ADR | Cerrada |
|------|--------|------|----|----|---------|
| 0 | ✅ Completada | `fase-0/saneamiento` | #2 | — | ✅ |
| 1 | ✅ Completada | `fase-1/persistencia-sql` | (contra `CD/CI`) | ADR-09 | ✅ |
| 2 | ✅ Completada | `fase-2/identity-login` | (contra `CD/CI`) | ADR-10 | ✅ |
| 3 | ✅ Completada | `fase-3/validacion` | (contra `CD/CI`) | ADR-11 | ✅ |
| 4 | ⬜ Pendiente | — | — | — | — |
| 5 | ⬜ Pendiente | — | — | — | — |
| 6 | ⬜ Pendiente | — | — | — | — |
| 7 | ⬜ Pendiente | — | — | — | — |
| 8 | ⬜ Pendiente | — | — | — | — |
| 9 | ⬜ Pendiente | — | — | — | — |
| 10 | ⬜ Pendiente | — | — | — | — |
