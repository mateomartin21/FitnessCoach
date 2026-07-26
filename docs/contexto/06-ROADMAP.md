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
Fase 4        Fase 5             Fase 6 ✅
Tracker       Catálogo de        Resiliencia IA
              ejercicios         (+ IA expandida)
   │              │                  │
   │              ▼                  │
   │         Fase 5.5 ✅             │
   │         Nutrición               │
   │         personalizada           │
   │              │                  │
   │              ▼                  │
   │         Fase 5.6 ✅             │
   │         Preferencias y          │
   │         adherencia              │
   │              │                  │
   │              │                  ▼
   │              │              Fase 7 ✅
   │              │              IA pulido
   │              │                  │
   └──────────────┴──────────────────┘
                  ▼
             Fase 8 ✅ Gamificación
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

> **✅ Cerrada el 24/07/2026** (rama `fase-4/tracker`, ADR-12) — seis commits: identidad y UTC (`d112705`), la vista que faltaba (`9c04380`), edición y borrado (`c26722c`), gráfica (`ce63aff`), entrenamientos y rachas (`8e9edb6`) y documentación. D-10, D-12 y D-23 resueltas. 95/95 pruebas en verde.
>
> **Los récords personales por ejercicio se movieron a la Fase 5**: dependen de que los ejercicios tengan identidad, y hoy están hardcodeados dentro de cada Strategy. Usar el nombre como clave habría sido frágil y la Fase 5 tendría que migrar esos datos.
>
> Deuda nueva: **D-25** (rachas contadas en la zona del servidor → Fase 10) y **D-26** (la API no cubre el tracker → Fase 10).

**Objetivo:** convertir el historial de peso en un tracker de verdad, al estilo Hevy/Jefit.

**Resuelve:** D-10, D-12, D-23

**Entregables:**
- **`Views/Progreso/Index.cshtml`, que hoy no existe (D-23):** el controlador ya devuelve `View(historial)` contra una vista ausente, así que la pantalla revienta. Es la base sobre la que se monta todo lo demás de esta fase.
- `Id` real para `RegistroProgreso` en el dominio (permite editar/borrar registros individuales)
- Todas las fechas en UTC; conversión solo al mostrar
- Registro de **entrenamientos completados**, no solo de peso
- Cálculo de rachas (días consecutivos)
- ~~Récords personales por ejercicio~~ → **movido a la Fase 5** (necesita la entidad `Ejercicio`)
- Gráfica de evolución del peso
- Vista de historial con edición y borrado

**Definition of Done:** un usuario puede registrar un entrenamiento, verlo en su historial, ver su racha actual y su gráfica de peso. Todo cubierto por pruebas.
**ADR:** ADR-12 — el tracker como historial de hechos, con reglas en la capa de aplicación.
**Rama sugerida:** `fase-4/tracker`

---

## Fase 5 — Catálogo de ejercicios y variedad

> **✅ Cerrada el 24/07/2026** (rama `fase-5/catalogo-ejercicios`, ADR-13) — seis commits: separación del modelo (`10f73da`), catálogo persistido (`e0257b3`), estrategias componiendo desde el catálogo (`a885b97`), vista con GIF e instrucciones (`7ff4aa8`), corrección de las instrucciones (`1c52d97`) y récords personales (`cab00e5`). 121/121 pruebas en verde.
>
> **1.323 ejercicios** en español con GIF e instrucciones, contra los ~30 nombres hardcodeados anteriores. Los récords personales heredados de la Fase 4 quedaron cerrados acá.
>
> Sobre la advertencia de abajo: los tests de `GeneradorRutinasService` **no necesitaron cambiar sus asserts** — solo se les inyectó un catálogo falso. Lo mismo con los del Decorator.
>
> Deuda nueva: **D-27** y **D-28** (alimentación, ver Fase 5.5), **D-29** (licencia de los GIFs).

**Objetivo:** contenido real. Que dos usuarios con el mismo objetivo no vean exactamente lo mismo.

**Entregables:**
- Entidad `Ejercicio` como concepto de primera clase (hoy es una clase suelta dentro de las estrategias, y encima vive en `Entrenamiento.cs`, cuyo nombre no coincide con el tipo), con: nombre, grupo muscular, nivel, equipo necesario, URL de GIF, instrucciones
- **Récords personales por ejercicio** (movido desde la Fase 4): una vez que `Ejercicio` tiene identidad, se puede registrar y comparar el mejor peso/repeticiones sin usar el nombre como clave
- Catálogo persistido en base de datos (con datos semilla), no hardcodeado dentro de cada Strategy
- Las estrategias pasan a **componer rutinas desde el catálogo** en vez de tener los ejercicios incrustados
- GIFs demostrativos, con énfasis en principiantes
- Suficiente variedad para rotación

**Definition of Done:** agregar un ejercicio nuevo no requiere tocar ninguna clase de Strategy. Los tests del Decorator siguen pasando sin cambios (por eso usan estrategias falsas).
**ADR:** ADR-13 — el catálogo desacoplado de las estrategias es un cambio de diseño relevante.
**Rama sugerida:** `fase-5/catalogo-ejercicios`

> ⚠ Es la fase con más impacto sobre las pruebas existentes. Los tests de `GeneradorRutinasService` que hoy verifican `rutina.Nivel` pueden necesitar ajuste — revisar antes de empezar.

---

## Fase 5.5 — Catálogo de alimentos y planes personalizados ✅

**Estado:** ✅ **Cerrada** el 24/07/2026 (rama `fase-5.5/nutricion-personalizada`, ADR-14). 227/227 pruebas.

**Objetivo:** que el plan de comidas deje de ser un folleto fijo y responda a las calorías reales de cada usuario. Es el espejo exacto de la Fase 5, pero en alimentación.

**Resolvió:** D-27, D-28

**Por qué existió esta fase:** al cerrar la Fase 5 quedó a la vista que la alimentación arrastraba los mismos problemas que los ejercicios acababan de resolver, más uno propio y peor: `CaloriasObjetivo = "1800-2000 kcal/día"` estaba escrito a mano en cada estrategia, así que **el plan ignoraba el cálculo calórico que la propia app le muestra al usuario en Perfil**.

**Entregado:**
- ✅ `CalculadorMacros`: reparte las calorías en proteína (por peso), grasa (% del total) y carbohidratos (por diferencia), con pisos de seguridad
- ✅ Entidad `Alimento` persistida con macros por 100 g; 67 alimentos sembrados desde USDA (volcado SR Legacy, dominio público) con imágenes de Wikimedia atribuidas
- ✅ Puerto `IRepositorioAlimentos` + adaptador SQL de solo lectura + doble en pruebas
- ✅ Las tres estrategias **componen** el plan desde el catálogo (`PlantillaComida` + `RolAlimento`) y escalan las porciones a los macros del usuario — murió el rango fijo (D-27)
- ✅ Sustituciones por equivalencia de macros en cada porción, acotadas por grupo de intercambio y momento del día
- ✅ Filtro de momento del día (que el desayuno no traiga tempeh con pasta) y descargo médico visible
- ✅ Pruebas que validan el JSON de la semilla y corren el generador contra el catálogo real con cinco perfiles distintos

**Definition of Done:** ✅ dos usuarios con requerimientos calóricos distintos reciben planes distintos y coherentes con el número que la app les muestra en Perfil. Agregar un alimento es una línea de JSON, sin tocar ninguna Strategy.
**ADR:** ADR-14.

---

## Fase 5.6 — Preferencias, exclusiones y adherencia ✅

**Estado:** ✅ **Cerrada** el 25/07/2026 (rama `fase-5.6/preferencias-adherencia`, ADR-15). 259/259 pruebas.

**Objetivo:** que el plan respete lo que la persona puede y quiere comer, y que pueda seguir si lo cumple. Cierra el apartado de nutrición.

**Dependió de:** Fase 5.5 (el catálogo, las etiquetas de dieta y el motor de composición ya estaban; esta fase los usó).

**Entregado:**
- ✅ `PreferenciasAlimentarias` (objeto de valor en el perfil): dietas seguidas (vegetariano, vegano, sin gluten, sin lactosa) y alimentos excluidos por slug, con la regla `Permite(Alimento)` y sus pruebas
- ✅ El motor filtra por preferencias **arriba de todo** en la selección, así que ni los fallbacks ni las sustituciones devuelven algo vetado
- ✅ Pantalla para editar preferencias; el POST solo acepta dietas conocidas y slugs del catálogo
- ✅ `RegistroComida` (diario) como colección owned del perfil, con snapshot de macros; `ServicioDiario` para registrar/borrar/resumir y `ResumenDiario` como cálculo puro (consumido vs objetivo)
- ✅ Pantalla de diario: registrar una comida del plan de un toque o cualquier alimento del catálogo, ver el día contra el objetivo, borrar, navegar por fecha
- ✅ Pruebas de que un vegetariano con alergia y un vegano reciben planes sin nada excluido, contra el catálogo real

**Definition of Done:** ✅ un usuario vegetariano con alergia declarada recibe un plan completo que nunca incluye lo excluido, y puede registrar lo que comió con seguimiento de macros del día.
**ADR:** ADR-15.

> Fue inmediatamente después de la 5.5 por la misma razón que aquella siguió a la 5: el catálogo, las etiquetas y el motor ya estaban hechos y probados; esta fase fue sobre todo filtrado y una entidad de registro. Cierra nutrición antes de pasar a la línea de IA.

---

## Fase 6 — Resiliencia de IA (ampliada) ✅

**Estado:** ✅ **Cerrada** el 25/07/2026 (rama `fase-6/resiliencia-ia`, ADR-16). 294/294 pruebas.

**Objetivo:** que si Gemini falla, el Lobo Coach no muera — y, por pedido del usuario, que la IA vea los datos reales, no invente y sea una capa sobre el sistema, no un chat aislado. Absorbió el grueso de la Fase 7.

**Resolvió:** D-09, D-20

**Entregado:**
- ✅ Puerto `IProveedorIA` (Domain/Ports); el controlador depende de `ICoachIA`, no del adaptador concreto
- ✅ Errores como `CoachIAException`, registrados con `ILogger` — nunca devueltos como texto (D-09)
- ✅ Personalidad del Lobo en Application, fuera del adaptador, con más carácter y reglas de no-invención (D-20)
- ✅ `CoachResiliente` (Chain of Responsibility) + `IFabricaProveedoresIA` (Factory)
- ✅ Cadena gratuita por capas: Gemini (2 modelos) → Groq/OpenRouter si hay clave (otra empresa, gratis) → offline por reglas (última garantía, sin red)
- ✅ `ArmadorContextoCoach`: contexto rico (perfil, plan, rutina, diario, récords) anclado al catálogo real de alimentos
- ✅ La IA como capa: endpoint `Analizar` + tarjeta "El Lobo analiza tu progreso" en la pantalla de Progreso, con datos reales

**Definition of Done:** ✅ prueba de fuego §7, punto 6 — con internet desconectado, el Lobo responde con gracia (offline) y la app no se cae.
**ADR:** ADR-16.

> Pendiente sin código: configurar una clave gratuita de Groq u OpenRouter (`Groq:ApiKey` / `OpenRouter:ApiKey` en user-secrets) para el respaldo inteligente de otra empresa. Sin ella, la cadena es Gemini → Gemini secundario → offline.

---

## Fase 7 — IA expandida (pulido) ✅

**Estado:** ✅ **Cerrada** el 25/07/2026 (rama `fase-7/ia-pulido`, ADR-17). 297/297 pruebas. El grueso ya se había hecho en la Fase 6 (contexto rico, análisis sobre datos reales, IA como capa); esta fase cerró los flecos.

**Dependió de:** Fase 4 (datos) y Fase 6 (IA confiable).

**Entregado:**
- ✅ Tarjeta de análisis del Lobo extraída a un partial reutilizable (`_AnalisisLobo.cshtml`) parametrizado por aspecto, en las pantallas de Progreso, Alimentación (dieta) y Rutina (rutina) — reusando el endpoint `Analizar` que ya lo soportaba
- ✅ Bloque `== ESTA SEMANA ==` en el contexto (entrenamientos de 7 días, racha y variación de peso), que mejora además todas las respuestas del chat
- ✅ Resumen semanal narrado en la voz del Lobo (aspecto `semana`), con su tarjeta en Progreso

**Definition of Done:** ✅ el análisis usa datos reales del usuario y degrada con gracia si la IA no está disponible (heredado de la Fase 6). La capacidad de análisis por aspecto, que estaba latente, quedó ofrecida en las pantallas.
**ADR:** ADR-17 — el Lobo en toda la app y el resumen semanal narrado.
**Rama sugerida:** `fase-7/ia-pulido`

---

## Fase 8 — Gamificación ✅

**Estado:** ✅ **Cerrada** el 26/07/2026 (rama `fase-8/gamificacion`, ADR-18). 323/323 pruebas.

**Objetivo:** las mecánicas de juego que sostienen la constancia.

**Dependió de:** Fase 4 (las mecánicas se alimentan de los datos del tracker).

**Entregado:**
- ✅ **Niveles / XP por constancia:** `CalculadorXP` y `CalculadorNivel` (curva creciente de RPG, títulos de lobo). La mejor racha da un bono, para premiar la constancia por sobre la acumulación (05-VISION §77)
- ✅ **Logros desbloqueables:** 12 logros anclados a hechos reales, con criterio medible (progreso, no solo sí/no) y reacción del Lobo cada uno
- ✅ **Misiones semanales:** 3 misiones medidas sobre los últimos 7 días, que se reinician con la ventana
- ✅ **El Lobo reacciona:** aviso en su voz al desbloquear un logro tras registrar un entrenamiento o un récord
- ✅ Pantalla estilo RPG (barra de XP, nivel, misiones, logros) y entrada en el menú

**Decisión de diseño (ADR-18):** todo se **deriva de los hechos** ya registrados —sin tablas nuevas ni estado de juego persistido—, así no puede desincronizarse y queda como lógica de dominio pura. `EstadisticasUsuario` (snapshot) + calculadores puros en Domain; `ServicioGamificacion` lo arma desde el perfil.

**Definition of Done:** ✅ las mecánicas se calculan desde datos reales y están cubiertas por pruebas (lógica de dominio pura, +26 en la fase).
**ADR:** ADR-18 — gamificación derivada de los hechos, sin estado paralelo.
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
**ADR:** ADR-18 — decisión de identidad visual y su implementación.
**Rama sugerida:** `fase-9/pixel-art`

---

## Fase 10 — Optimización y cierre

**Objetivo:** el pulido final.

**Entregables:**
- Rate limiter listo para producción (D-24): almacén compartido en vez de memoria del proceso, y `UseForwardedHeaders` para no contar todo el tráfico bajo la IP del balanceador
- Zona horaria del usuario (D-25): guardarla en el perfil y contar las rachas en su calendario, no en el del servidor
- Completar la API REST con el tracker (D-26): edición y borrado de registros, entrenamientos y rachas
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
| 4 | ✅ Completada | `fase-4/tracker` | (contra `CD/CI`) | ADR-12 | ✅ |
| 5 | ✅ Completada | `fase-5/catalogo-ejercicios` | (contra `CD/CI`) | ADR-13 | ✅ |
| 5.5 | ✅ Completada | `fase-5.5/nutricion-personalizada` | (contra `CD/CI`) | ADR-14 | ✅ |
| 5.6 | ✅ Completada | `fase-5.6/preferencias-adherencia` | (contra `CD/CI`) | ADR-15 | ✅ |
| 6 | ✅ Completada | `fase-6/resiliencia-ia` | (contra `CD/CI`) | ADR-16 | ✅ |
| 7 | ✅ Completada | `fase-7/ia-pulido` | (contra `CD/CI`) | ADR-17 | ✅ |
| 8 | ✅ Completada | `fase-8/gamificacion` | (contra `CD/CI`) | ADR-18 | ✅ |
| 9 | ⬜ Pendiente | — | — | — | — |
| 10 | ⬜ Pendiente | — | — | — | — |
