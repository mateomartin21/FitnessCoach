# 04 — Deuda técnica y bugs

> **Documento vivo.** Es el inventario honesto de lo que está mal. Cuando algo se resuelve se marca ✅ con la fase que lo resolvió — **no se borra la línea**, el historial sirve para los ADRs y para la sustentación del proyecto.
>
> Última revisión completa del código: **22/07/2026**, rama `CD/CI`.

## Resumen

| Severidad | Total | Abiertas |
|-----------|-------|----------|
| 🔴 Crítica (seguridad / pérdida de datos) | 7 | 5 |
| 🟠 Alta (bug funcional o riesgo real) | 5 | 4 |
| 🟡 Media (calidad, mantenibilidad) | 8 | 1 |
| **Total** | **20** | **10** |

> **Resueltas hasta ahora:** Fase 0 → D-08, D-13, D-14, D-15, D-16, D-17, D-18, D-19. Fase 1 → D-03, D-06. Queda abierta de calidad solo D-20 (prompt del Lobo, Fase 6).

---

## 🔴 Críticas

### D-01 · Los endpoints REST están completamente abiertos
**Dónde:** `Web/ApiControllers/UsuariosApiController.cs`, `Web/ApiControllers/ProgresoApiController.cs`
**Qué pasa:** ningún atributo `[Authorize]`. Cualquiera con la URL puede leer el perfil de cualquier usuario (`GET /api/usuarios/{id}`), leer su historial de peso (`GET /api/usuarios/{id}/progreso`), crear perfiles ilimitados (`POST /api/usuarios`) y escribir registros de progreso en la cuenta de otro (`POST /api/usuarios/{id}/progreso`).
**Riesgo:** IDOR total. Es el hallazgo más grave del proyecto.
**Resolución:** Fase 2. `[Authorize]` + resolver el dueño desde la identidad autenticada, nunca desde la URL.
**Estado:** ⬜ Abierta

### D-02 · Usuario único hardcodeado (`Id = 1`)
**Dónde:** `PerfilController.Index`, `PerfilController.GuardarPerfil` (fija `Id = 1`), `ProgresoController.Index`, `ProgresoController.RegistrarPeso`, `IaCoachController.Consultar`
**Qué pasa:** toda la aplicación opera sobre un único perfil fijo. No existe el concepto de "mi cuenta".
**Riesgo:** si dos personas usan la app, comparten y se sobrescriben el perfil mutuamente. Además `GuardarPerfil` fuerza `Id = 1`, así que cualquier alta sobrescribe al usuario existente.
**Resolución:** Fase 2.
**Estado:** ⬜ Abierta

### D-03 · La base de datos existe pero no se usa
**Dónde:** `Program.cs`
**Qué pasa:** `ApplicationDbContext` está registrado y la migración `InitialCreate` está aplicada, pero el puerto `IRepositorioUsuario` sigue apuntando a `RepositorioUsuarioMemoria` (`AddSingleton`). Ningún repositorio consume el `DbContext`.
**Riesgo:** **todos los datos se pierden en cada reinicio del servidor.** El ADR-07 declara esta deuda como "✅ Resuelta", lo cual es incorrecto — sí dejó anotado el pendiente de `RepositorioUsuarioSql` en su sección final, pero el ADR se lee como si la persistencia ya funcionara.
**Resolución:** Fase 1.
**Estado:** ✅ Resuelta en Fase 1 (commit `b85dae1`, ADR-09). `RepositorioUsuarioSql` sobre `ApplicationDbContext`, registrado como `Scoped`. Datos verificados tras reinicio.

### D-04 · Sin validación de entrada en ningún modelo
**Dónde:** `Domain/Models/UsuarioPerfil.cs`, `Domain/Models/RegistroProgreso.cs`, `PerfilController.GuardarPerfil`
**Qué pasa:** cero anotaciones de validación. `GuardarPerfil` recibe parámetros sueltos y ni siquiera consulta `ModelState`. Se acepta peso negativo, edad 0 o 500, estatura 0, nombre vacío.
**Riesgo:** datos basura en la base y cálculos sin sentido. Con `EstaturaCm = 0` el cálculo calórico devuelve un número absurdo **sin lanzar error** — bug silencioso.
**Resolución:** Fase 3.
**Estado:** ⬜ Abierta

### D-05 · Sin protección CSRF
**Dónde:** `PerfilController.GuardarPerfil`, `ProgresoController.RegistrarPeso`
**Qué pasa:** acciones `[HttpPost]` que modifican estado, sin `[ValidateAntiForgeryToken]`.
**Riesgo:** una vez que exista login, un sitio externo podría hacer que el navegador del usuario autenticado modifique su perfil sin su consentimiento.
**Resolución:** Fase 2.
**Estado:** ⬜ Abierta

### D-06 · Repositorio singleton con estado mutable no sincronizado
**Dónde:** `Program.cs` (`AddSingleton`) + `RepositorioUsuarioMemoria`
**Qué pasa:** un `List<UsuarioPerfil>` compartido entre todas las peticiones sin ningún bloqueo. La asignación de ID (`usuario.Id = _usuarios.Count + 1`) no es atómica.
**Riesgo:** condición de carrera real — dos peticiones simultáneas pueden generar el mismo ID o corromper la lista. Se reproduce guardando desde dos pestañas a la vez.
**Resolución:** Fase 1 (al pasar a `Scoped` con `DbContext` desaparece).
**Estado:** ✅ Resuelta en Fase 1 (commit `b85dae1`, ADR-09). `AddSingleton` → `AddScoped`; el estado ya no vive en una `List<>` compartida sino en SQL Server.

### D-07 · `UseAuthentication()` ausente
**Dónde:** `Program.cs`
**Qué pasa:** el pipeline llama a `app.UseAuthorization()` pero nunca a `app.UseAuthentication()`. Sin el primero, el segundo no tiene identidad que evaluar — la línea existe pero no hace nada.
**Riesgo:** da falsa sensación de que hay seguridad configurada.
**Resolución:** Fase 2.
**Estado:** ⬜ Abierta

---

## 🟠 Altas

### D-08 · Vulnerabilidad conocida en `Microsoft.OpenApi 2.0.0`
**Dónde:** `FitnessCoach.csproj` (transitiva vía `Microsoft.AspNetCore.OpenApi 10.0.9`)
**Qué pasa:** cada `dotnet build` emite `warning NU1903` — vulnerabilidad de severidad **alta** conocida ([GHSA-v5pm-xwqc-g5wc](https://github.com/advisories/GHSA-v5pm-xwqc-g5wc)).
**Riesgo:** además del riesgo en sí, es un warning permanente en el log del pipeline de CI, lo que entrena a ignorar los warnings.
**Resolución:** Fase 0 — actualizar el paquete.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). `dotnet build` ya no emite `NU1903`.

### D-09 · Los errores de Gemini se devuelven como si fueran consejos
**Dónde:** `Infrastructure/Adapters/GeminiCoachService.cs`
**Qué pasa:** ante un fallo de la API, el método devuelve un `string` con el texto del error (`"Error de conexion con el coach: ..."`) por el mismo canal que una respuesta válida. El `catch (Exception)` final es mudo: no registra nada.
**Riesgo:** el llamador no puede distinguir éxito de fallo, lo cual **bloquea por completo** el mecanismo de fallback planeado para la Fase 6. Además, los fallos no quedan registrados en ningún lado.
**Resolución:** Fase 6 (rediseño con excepción propia o tipo resultado).
**Estado:** ⬜ Abierta

### D-10 · Inconsistencia en el manejo de fechas
**Dónde:** `ProgresoController.RegistrarPeso` usa `DateTime.Now`; `ProgresoApiController.AgregarRegistro` usa `DateTime.UtcNow`
**Qué pasa:** dos caminos escriben la misma colección con criterios horarios distintos.
**Riesgo:** el historial se ordena incorrectamente al mezclar registros creados por la vista y por el API (desfase de horas según la zona del servidor).
**Resolución:** Fase 4 — todo a UTC, conversión solo en la vista.
**Estado:** ⬜ Abierta

### D-11 · `POST /api/usuarios` permite crear perfiles ilimitados
**Dónde:** `UsuariosApiController.Crear`
**Qué pasa:** sin autenticación ni límite. Cada llamada crea un perfil nuevo.
**Riesgo:** vector trivial de denegación de servicio / llenado de la base.
**Resolución:** Fase 2 (junto con D-01; además el índice único de la Fase 2 impide más de un perfil por usuario).
**Estado:** ⬜ Abierta

### D-12 · `RegistroProgreso` sin identidad propia en el dominio
**Dónde:** `Domain/Models/RegistroProgreso.cs` + `ApplicationDbContext.OnModelCreating`
**Qué pasa:** el `Id` existe solo como *shadow property* de EF Core. El dominio no puede referirse a un registro individual.
**Riesgo:** hoy no molesta, pero bloquea funcionalidad de la Fase 4 (editar o borrar un registro específico del historial).
**Resolución:** Fase 4.
**Estado:** ⬜ Abierta

---

## 🟡 Medias — calidad y mantenibilidad

### D-13 · Archivos fuente en ISO-8859-1 en vez de UTF-8
**Dónde:** prácticamente todos los `.cs` del proyecto
**Qué pasa:** los acentos y guiones largos aparecen corruptos. Ejemplos reales: `"Clculo del Metabolismo Basal"` (falta la á), `"cÃ¡lculo calÃ³rico"` en los comentarios de `Program.cs`, `"PATRON STRATEGY "` con el guion perdido.
**Riesgo:** afecta la legibilidad y se agrava con cada herramienta que toque los archivos. Si algún día un texto de estos llega a la UI, llega corrupto.
**Resolución:** Fase 0 — conversión masiva a UTF-8.
**Estado:** ✅ Resuelta. La Fase 0 convirtió la mayoría; los 5 archivos que quedaban en Windows-1252 (`ProgresoController.cs`, `RutinasController.cs`, `CalculadorCaloricoService.cs`, `GeneradorRutinasService.cs`, `ObjetivoGanarMusculo.cs`) se convirtieron a UTF-8 el 23/07/2026 (`iconv -f WINDOWS-1252 -t UTF-8`). Todo el árbol `.cs` queda en UTF-8.

### D-14 · Typo en el namespace: `Repositoriess`
**Dónde:** `Infrastructure/Repositories/RepositorioUsuarioMemoria.cs` → `namespace FitnessCoach.Infrastructure.Repositoriess`
**Qué pasa:** doble "s". Se propaga a `Program.cs`, que lo importa mal escrito.
**Resolución:** Fase 0.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). Namespace corregido a `FitnessCoach.Infrastructure.Repositories`.

### D-15 · Nombre de archivo malformado
**Dónde:** `Application/Services/CalculadorCaloricoServicecs.cs`
**Qué pasa:** falta el punto antes de la extensión — debería ser `CalculadorCaloricoService.cs`.
**Resolución:** Fase 0.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). Archivo renombrado a `CalculadorCaloricoService.cs`.

### D-16 · Nombre de archivo ≠ nombre del tipo (×2)
**Dónde:**
- `Application/Services/GeneradorRutinaService.cs` contiene `GeneradorRutinasService` (falta la "s")
- `Application/Services/IGeneradorRutina.cs` contiene `IGeneradorRutinas` (falta la "s")

**Riesgo:** dificulta encontrar los archivos y rompe la convención de `03-ESTANDARES.md` §4.
**Resolución:** Fase 0.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). Archivos renombrados a `GeneradorRutinasService.cs` e `IGeneradorRutinas.cs`.

### D-17 · Archivos `Class1.cs` de plantilla sin borrar
**Dónde:** `FitnessCoach.Domain/Class1.cs`, `FitnessCoach.Application/Class1.cs`, `FitnessCoach.Infrastructure/Class1.cs`
**Qué pasa:** sobras de `dotnet new classlib` que nunca se eliminaron.
**Resolución:** Fase 0.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). Los tres `Class1.cs` eliminados.

### D-18 · `ErrorViewModel` vive en `Domain`
**Dónde:** `Domain/Models/ErrorViewModel.cs`, consumido por `HomeController.Error()`
**Qué pasa:** es un modelo de presentación (guarda un `RequestId` de HTTP), no un concepto del negocio. Está en la capa equivocada.
**Riesgo:** violación menor pero real de la regla de dependencias de `02-ARQUITECTURA.md`. Es exactamente el tipo de detalle por el que preguntan en una sustentación.
**Resolución:** Fase 0 — mover a `Models/` del proyecto web.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). `ErrorViewModel` movido fuera de `Domain`.

### D-19 · Los ADR-07 tienen markdown escapado
**Dónde:** `docs/ADR-07-FitnessCoach.md`, `docs/ADR-07-deuda-tecnica.md`
**Qué pasa:** el contenido está guardado con escapes (`\#`, `\*\*`, `\-`, `\|`), así que en GitHub se ve el texto plano con las barras invertidas en vez de renderizar encabezados y tablas.
**Riesgo:** el documento formal más importante del proyecto se ve roto para quien lo evalúe.
**Resolución:** Fase 0.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). ADR-07 renderiza correctamente en GitHub.

### D-20 · El prompt del Lobo Coach está hardcodeado en el adaptador
**Dónde:** `Infrastructure/Adapters/GeminiCoachService.cs`
**Qué pasa:** el prompt que define la personalidad del Lobo Coach está incrustado como string dentro del adaptador HTTP. Mezcla "cómo hablo con la API de Google" con "quién es el Lobo Coach".
**Riesgo:** la personalidad del personaje es central a la visión del producto (`05-VISION-PRODUCTO.md`) y va a evolucionar mucho. Tenerla dentro del adaptador significa que cambiar de proveedor de IA implicaría reescribir la personalidad, y viceversa.
**Resolución:** Fase 6 — extraer la construcción del prompt fuera del adaptador.
**Estado:** ⬜ Abierta

---

## Cómo registrar deuda nueva

Al detectar algo, se agrega con este formato:

```markdown
### D-NN · Título corto y descriptivo
**Dónde:** ruta/al/archivo.cs
**Qué pasa:** descripción objetiva, sin adjetivos
**Riesgo:** qué se rompe en la práctica si no se paga
**Resolución:** Fase N / propuesta concreta
**Estado:** ⬜ Abierta
```

Al resolverla: `**Estado:** ✅ Resuelta en Fase N (commit `abc1234`)` — y se actualiza la tabla de resumen del inicio.
