# 04 — Deuda técnica y bugs

> **Documento vivo.** Es el inventario honesto de lo que está mal. Cuando algo se resuelve se marca ✅ con la fase que lo resolvió — **no se borra la línea**, el historial sirve para los ADRs y para la sustentación del proyecto.
>
> Última revisión completa del código: **22/07/2026**, rama `CD/CI`. Actualizado el **26/07/2026** al cerrar la Fase 9 (rama `fase-9/identidad-pixel`, ADR-19). La Fase 8 (gamificación) no cerró ni abrió deuda formal. La Fase 9 (identidad pixel art) abrió dos deudas cosméticas/de limpieza: D-30 (halo residual en sprites) y D-31 (PNGs de branding sin uso).

## Resumen

| Severidad | Total | Abiertas |
|-----------|-------|----------|
| 🔴 Crítica (seguridad / pérdida de datos) | 7 | 0 |
| 🟠 Alta (bug funcional o riesgo real) | 9 | 1 |
| 🟡 Media (calidad, mantenibilidad) | 15 | 5 |
| **Total** | **31** | **6** |

> **Resueltas hasta ahora:** Fase 0 → D-08, D-13, D-14, D-15, D-16, D-17, D-18, D-19. Fase 1 → D-03, D-06. Fase 2 → D-01, D-02, D-05, D-07, D-11. Fase 3 → D-04, D-21, D-22. Fase 4 → D-10, D-12, D-23. Fase 5.5 → D-27, D-28. Fase 6 → D-09, D-20.
>
> **No queda ninguna deuda crítica abierta.** La única alta abierta es D-26 (la API no cubre el tracker, Fase 10). Las cinco medias abiertas: D-24 (rate limiter en memoria), D-25 (zona horaria del servidor), D-29 (licencia de los GIFs), D-30 (halo residual en sprites) y D-31 (PNGs de branding sin uso).
>
> **Deuda nueva detectada en la Fase 4:** D-25 y D-26. **En la Fase 5:** D-27, D-28 y D-29. **En la Fase 9:** D-30 y D-31.

---

## 🔴 Críticas

### D-01 · Los endpoints REST están completamente abiertos
**Dónde:** `Web/ApiControllers/UsuariosApiController.cs`, `Web/ApiControllers/ProgresoApiController.cs`
**Qué pasa:** ningún atributo `[Authorize]`. Cualquiera con la URL puede leer el perfil de cualquier usuario (`GET /api/usuarios/{id}`), leer su historial de peso (`GET /api/usuarios/{id}/progreso`), crear perfiles ilimitados (`POST /api/usuarios`) y escribir registros de progreso en la cuenta de otro (`POST /api/usuarios/{id}/progreso`).
**Riesgo:** IDOR total. Es el hallazgo más grave del proyecto.
**Resolución:** Fase 2. `[Authorize]` + resolver el dueño desde la identidad autenticada, nunca desde la URL.
**Estado:** ✅ Resuelta en Fase 2 (commit `926ae0c`, ADR-10). Las rutas pasaron a `/api/perfil` y `/api/perfil/progreso`: **el id del usuario dejó de existir como parámetro de entrada**, así que no hay nada que manipular ni comprobación que olvidar. `[Authorize]` en ambos controladores y sin sesión responden `401` en vez de redirigir al login.

### D-02 · Usuario único hardcodeado (`Id = 1`)
**Dónde:** `PerfilController.Index`, `PerfilController.GuardarPerfil` (fija `Id = 1`), `ProgresoController.Index`, `ProgresoController.RegistrarPeso`, `IaCoachController.Consultar`
**Qué pasa:** toda la aplicación opera sobre un único perfil fijo. No existe el concepto de "mi cuenta".
**Riesgo:** si dos personas usan la app, comparten y se sobrescriben el perfil mutuamente. Además `GuardarPerfil` fuerza `Id = 1`, así que cualquier alta sobrescribe al usuario existente.
**Resolución:** Fase 2.
**Estado:** ✅ Resuelta en Fase 2 (commit `1dc0e52`, ADR-10). `ServicioPerfilUsuario` resuelve el perfil desde `IdentityUserId`; no queda ningún `ObtenerPorId(1)` en el código. Un índice único filtrado sobre `IdentityUserId` garantiza en la base la invariante "un usuario = un perfil".

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
**Estado:** ✅ Resuelta en Fase 3 (commits `565965d`, `5e842c9` y `c9e0114`, ADR-11). Validación en dos capas: anotaciones en las entidades y ViewModels (rangos centralizados en `RangosPerfil`), y guardas en `CalcularCaloriasDiarias` que lanzan `ArgumentOutOfRangeException` en vez de devolver un número falso. `GuardarPerfil` recibe un ViewModel y verifica `ModelState` antes de tocar nada. 32 pruebas nuevas cubren los casos límite.

### D-05 · Sin protección CSRF
**Dónde:** `PerfilController.GuardarPerfil`, `ProgresoController.RegistrarPeso`
**Qué pasa:** acciones `[HttpPost]` que modifican estado, sin `[ValidateAntiForgeryToken]`.
**Riesgo:** una vez que exista login, un sitio externo podría hacer que el navegador del usuario autenticado modifique su perfil sin su consentimiento.
**Resolución:** Fase 2.
**Estado:** ✅ Resuelta en Fase 2 (commits `3a078b6` y `1dc0e52`, ADR-10). `[ValidateAntiForgeryToken]` en `PerfilController.GuardarPerfil`, `ProgresoController.RegistrarPeso` y las tres acciones POST de `AccountController`. **Excepción:** `IaCoachController.Consultar` quedó sin el atributo — registrada aparte como D-21.

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
**Estado:** ✅ Resuelta en Fase 2 (commit `bb8b0f9`, ADR-10). `app.UseAuthentication()` agregado antes de `app.UseAuthorization()`, con Identity registrado sobre `ApplicationDbContext`.

---

## 🟠 Altas

### D-08 · Vulnerabilidad conocida en `Microsoft.OpenApi 2.0.0`
**Dónde:** `FitnessCoach.csproj` (transitiva vía `Microsoft.AspNetCore.OpenApi 10.0.9`)
**Qué pasa:** cada `dotnet build` emite `warning NU1903` — vulnerabilidad de severidad **alta** conocida ([GHSA-v5pm-xwqc-g5wc](https://github.com/advisories/GHSA-v5pm-xwqc-g5wc)).
**Riesgo:** además del riesgo en sí, es un warning permanente en el log del pipeline de CI, lo que entrena a ignorar los warnings.
**Resolución:** Fase 0 — actualizar el paquete.
**Estado:** ✅ Resuelta en Fase 0 (commit `b9031bb`). `dotnet build` ya no emite `NU1903`.

### D-27 · El plan de alimentación ignora las calorías calculadas del usuario
**Dónde:** `Domain/Patterns/Strategy/Alimentacion/*.cs` → `CaloriasObjetivo = "1800-2000 kcal/día"`
**Qué pasa:** cada estrategia de alimentación trae un rango calórico fijo escrito a mano. `CalculadorCaloricoService` calcula el requerimiento real de cada persona (Mifflin-St Jeor + multiplicador del objetivo), la app lo muestra en la pantalla de Perfil, y el plan de comidas lo ignora: dos usuarios con 1900 y 2600 kcal calculadas reciben exactamente el mismo plan.
**Riesgo:** la app se contradice a sí misma en pantallas contiguas, y el plan que entrega no sirve para el objetivo de quien lo recibe. Es el consejo nutricional lo que queda mal, no solo el código.
**Resolución:** que la estrategia reciba las calorías objetivo del usuario y escale las porciones, en lugar de declararlas fijas. Detectada el 24/07/2026 al cerrar la Fase 5.
**Estado:** ✅ Resuelta en Fase 5.5 (commits `3e35f64` y `c620f5e`, ADR-14). `CalculadorMacros` reparte las calorías calculadas en gramos de proteína/grasa/carbohidrato (proteína por peso, grasa como % del total, carbohidratos por diferencia), y `EstrategiaAlimentacionBase` escala las porciones de cada comida a esos macros. El rango fijo `"1800-2000 kcal/día"` dejó de existir: dos usuarios con distinto peso reciben planes distintos, verificado contra el catálogo real con cinco perfiles.

### D-09 · Los errores de Gemini se devuelven como si fueran consejos
**Dónde:** `Infrastructure/Adapters/GeminiCoachService.cs`
**Qué pasa:** ante un fallo de la API, el método devuelve un `string` con el texto del error (`"Error de conexion con el coach: ..."`) por el mismo canal que una respuesta válida. El `catch (Exception)` final es mudo: no registra nada.
**Riesgo:** el llamador no puede distinguir éxito de fallo, lo cual **bloquea por completo** el mecanismo de fallback planeado para la Fase 6. Además, los fallos no quedan registrados en ningún lado.
**Resolución:** Fase 6 (rediseño con excepción propia o tipo resultado).
**Estado:** ✅ Resuelta en Fase 6 (commits `706d84a`, `8ef5209` y `6371090`, ADR-16). `IProveedorIA` lanza `CoachIAException` ante cualquier fallo en vez de devolver el error como texto; `CoachResiliente` distingue el fallo, lo registra con `ILogger` y pasa al siguiente proveedor. Si todos caen, el Lobo responde con su frase de "sin señal", nunca un error crudo.

### D-10 · Inconsistencia en el manejo de fechas
**Dónde:** `ProgresoController.RegistrarPeso` usa `DateTime.Now`; `ProgresoApiController.AgregarRegistro` usa `DateTime.UtcNow`
**Qué pasa:** dos caminos escriben la misma colección con criterios horarios distintos.
**Riesgo:** el historial se ordena incorrectamente al mezclar registros creados por la vista y por el API (desfase de horas según la zona del servidor).
**Resolución:** Fase 4 — todo a UTC, conversión solo en la vista.
**Estado:** ✅ Resuelta en Fase 4 (commit `d112705`, ADR-12). Todo se escribe con `DateTime.UtcNow` **y** el mapeo de EF marca la fecha como UTC al leerla: sin eso volvía como `Unspecified` y el `ToLocalTime()` de la vista no convertía nada, así que el arreglo se habría visto completo en el código y roto en pantalla.

### D-11 · `POST /api/usuarios` permite crear perfiles ilimitados
**Dónde:** `UsuariosApiController.Crear`
**Qué pasa:** sin autenticación ni límite. Cada llamada crea un perfil nuevo.
**Riesgo:** vector trivial de denegación de servicio / llenado de la base.
**Resolución:** Fase 2 (junto con D-01; además el índice único de la Fase 2 impide más de un perfil por usuario).
**Estado:** ✅ Resuelta en Fase 2 (commit `926ae0c`, ADR-10). El endpoint se eliminó: el perfil se crea solo, la primera vez que el usuario autenticado entra (`ServicioPerfilUsuario.ObtenerOCrear`). El índice único filtrado hace imposible el duplicado aunque el alta se invocara dos veces.

### D-26 · La API REST no cubre el tracker
**Dónde:** `Web/ApiControllers/ProgresoApiController.cs`
**Qué pasa:** la Fase 4 agregó edición y borrado de registros, entrenamientos completados y rachas, pero **solo por la vista MVC**. La API sigue con lo que definió el ADR-10: listar el historial, ver el último y agregar un registro. No expone `PUT`/`DELETE` de un registro concreto ni nada de entrenamientos.
**Riesgo:** ninguno de seguridad — es superficie que no existe. El problema es de coherencia: la API quedó siendo una vista parcial y desactualizada del producto, y `03-ESTANDARES.md` §1.5 ya anticipa cómo debería resolverse el caso de una ruta con id (verificar pertenencia y responder `404`).
**Resolución:** Fase 10, o antes si aparece un consumidor real (la app móvil listada como idea fuera de alcance). Al hacerlo, reusar `ServicioProgreso` y `ServicioEntrenamientos`, que ya tienen las reglas y el aislamiento por cuenta resueltos.
**Estado:** ⬜ Abierta

### D-12 · `RegistroProgreso` sin identidad propia en el dominio
**Dónde:** `Domain/Models/RegistroProgreso.cs` + `ApplicationDbContext.OnModelCreating`
**Qué pasa:** el `Id` existe solo como *shadow property* de EF Core. El dominio no puede referirse a un registro individual.
**Riesgo:** hoy no molesta, pero bloquea funcionalidad de la Fase 4 (editar o borrar un registro específico del historial).
**Resolución:** Fase 4.
**Estado:** ✅ Resuelta en Fase 4 (commit `d112705`, ADR-12). `RegistroProgreso.Id` es ahora una propiedad del dominio con `HasKey(r => r.Id)`. **No requirió cambio de esquema** — la columna ya existía —, lo que confirma que era una deuda de modelado y no de base de datos.

### D-23 · La vista de Progreso no existe
**Dónde:** `ProgresoController.Index` hace `return View(historial)`, pero no hay ningún `Views/Progreso/Index.cshtml` en el repo
**Qué pasa:** entrar a `/Progreso` lanza `InvalidOperationException: The view 'Index' was not found`. `RegistrarPeso` redirige a esa misma acción, así que el flujo de registrar peso por la web tampoco termina. Pasó inadvertido porque el menú de `_Layout` no enlaza a Progreso: solo se llega escribiendo la URL a mano.
**Riesgo:** una pantalla del producto directamente no funciona, y el único camino que queda para registrar peso es el API. Detectada el 24/07/2026 al empezar la Fase 3.
**Resolución:** Fase 4 — el tracker construye esa pantalla completa (gráfica de peso, historial con edición y borrado, rachas). Hacer una vista provisional en la Fase 3 sería trabajo que la Fase 4 tira. `ProgresoController` ya quedó con `ModelState` y `TempData["ErrorProgreso"]` listos para cuando la vista exista.
**Estado:** ✅ Resuelta en Fase 4 (commit `9c04380`, ADR-12). La pantalla existe con historial, formulario validado, entrenamientos, rachas y gráfica. **Se agregó además el enlace en el menú**, que era la razón de fondo de que el bug sobreviviera: la pantalla no era alcanzable desde ningún lado.

### D-22 · Login sin bloqueo por intentos fallidos
**Dónde:** `Controllers/AccountController.cs` → `PasswordSignInAsync(..., lockoutOnFailure: false)`
**Qué pasa:** Identity trae bloqueo temporal de cuenta tras N intentos fallidos, pero se registró desactivado. Nada limita la cantidad de contraseñas que se pueden probar contra una cuenta.
**Riesgo:** fuerza bruta sin fricción, agravado porque la política de contraseñas es laxa (`RequiredLength = 6`, sin exigir caracteres no alfanuméricos). Con un correo válido conocido, probar contraseñas comunes es cuestión de tiempo. Detectada al escribir el ADR-10.
**Resolución:** Fase 3 — `lockoutOnFailure: true` y configurar `options.Lockout` (ventana y número de intentos); endurecer de paso la política de contraseñas.
**Estado:** ✅ Resuelta en Fase 3 (commits `d5c1670` y `78183fb`, ADR-11). Bloqueo de 15 minutos tras 5 fallos, contraseñas de 8 caracteres con mayúscula/minúscula/número, **y** un rate limiter de 10 envíos por minuto y por IP sobre login y registro — el bloqueo de Identity cuenta por cuenta y por sí solo no frena el *password spraying*.

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

### D-28 · Los planes de alimentación están hardcodeados dentro de las estrategias
**Dónde:** `Domain/Patterns/Strategy/Alimentacion/AlimentacionPerderPeso.cs`, `AlimentacionGanarMusculo.cs`, `AlimentacionRecomposicion.cs`
**Qué pasa:** exactamente el mismo problema que la Fase 5 resolvió para los ejercicios, pero en alimentación. Las comidas viven incrustadas en cada Strategy y los alimentos son `List<string>` de texto plano: no hay entidad `Alimento`, ni catálogo, ni macros por alimento (los macros están sumados a mano por comida).
**Riesgo:** agregar o cambiar una comida obliga a editar una clase de dominio y recompilar; no hay variedad ni rotación posible (todos los usuarios con el mismo objetivo comen literalmente lo mismo todos los días); y no se pueden sustituir alimentos por alergias, preferencias o disponibilidad.
**Resolución:** replicar el patrón del catálogo de ejercicios — entidad `Alimento` persistida con macros, y estrategias que **componen** el plan desde el catálogo. Se hace junto con D-27, que necesita esa estructura para escalar porciones. Detectada el 24/07/2026 al cerrar la Fase 5.
**Estado:** ✅ Resuelta en Fase 5.5 (commits `1bc816d`, `c620f5e` y `ce69c66`, ADR-14). Entidad `Alimento` persistida con macros por 100 g (67 alimentos sembrados desde USDA), puerto `IRepositorioAlimentos` con adaptador SQL, y las tres estrategias pasaron a declarar la estructura del día (`PlantillaComida` + `RolAlimento`) mientras `EstrategiaAlimentacionBase` compone el plan desde el catálogo. Agregar un alimento es una línea de JSON. Además trae sustituciones por equivalencia de macros, que la estructura de texto plano anterior hacía imposibles.

### D-29 · Los GIFs del catálogo dependen de un CDN externo con licencia poco clara
**Dónde:** columna `UrlGif` de la tabla `Ejercicios`, apuntando a `cdn.jsdelivr.net/gh/JahelCuadrado/ExerciseGymGifsDB@v1.1.0`
**Qué pasa:** los 1323 GIFs se sirven desde un repositorio de terceros vía jsDelivr. El propio repositorio aclara que *"los GIFs pertenecen a sus respectivos autores"* y que solo aporta "una capa de organización", así que los derechos de las animaciones no están claros. Además la app depende de ese CDN para mostrarlas.
**Riesgo:** el técnico está acotado — la URL está pineada a una versión y, si el CDN falla, la cadena de medios degrada a placeholder sin romper la pantalla (ADR-13). El legal es el que importa: para un trabajo académico es bajo, pero si el proyecto se publicara habría que revisar la procedencia de cada GIF o reemplazarlos.
**Resolución:** decisión de producto, no técnica. Si el proyecto sale del ámbito académico: material propio, banco con licencia explícita, o quitar las animaciones y dejar solo instrucciones y enlace a video.
**Estado:** ⬜ Abierta

### D-20 · El prompt del Lobo Coach está hardcodeado en el adaptador
**Dónde:** `Infrastructure/Adapters/GeminiCoachService.cs`
**Qué pasa:** el prompt que define la personalidad del Lobo Coach está incrustado como string dentro del adaptador HTTP. Mezcla "cómo hablo con la API de Google" con "quién es el Lobo Coach".
**Riesgo:** la personalidad del personaje es central a la visión del producto (`05-VISION-PRODUCTO.md`) y va a evolucionar mucho. Tenerla dentro del adaptador significa que cambiar de proveedor de IA implicaría reescribir la personalidad, y viceversa.
**Resolución:** Fase 6 — extraer la construcción del prompt fuera del adaptador.
**Estado:** ✅ Resuelta en Fase 6 (commits `706d84a` y `c679077`, ADR-16). La personalidad vive en `PersonalidadLoboCoach` (Application): arma el prompt y guarda la respuesta de "sin señal". El adaptador de Gemini solo recibe el prompt ya armado. De paso se le dio más carácter al Lobo y reglas de no-invención ancladas al catálogo.

### D-21 · `IaCoachController.Consultar` es un POST sin `[ValidateAntiForgeryToken]`
**Dónde:** `Controllers/IaCoachController.cs` (acción `Consultar`), consumida por `fetch()` desde `Views/IaCoach/Index.cshtml`
**Qué pasa:** al cerrar D-05 en la Fase 2 se cubrieron las acciones POST de formularios, pero ésta quedó fuera porque no viene de un formulario Razor: la llama JavaScript con `Content-Type: application/json`.
**Riesgo:** bajo en la práctica — un POST cross-origin con ese `Content-Type` dispara *preflight* CORS, que falla al no haber política que lo permita. Pero es una excepción no declarada a la regla de `03-ESTANDARES.md` §5, y depende de un detalle del navegador en vez de una defensa explícita. Consumir la acción gasta cuota de la API de Gemini.
**Resolución:** Fase 3 — enviar el token antiforgery en la cabecera `RequestVerificationToken` desde el `fetch()` y validarlo en la acción, o mover la acción a la API con su propio esquema.
**Estado:** ✅ Resuelta en Fase 3 (commit `d5c1670`, ADR-11). La vista emite el token con `@Html.AntiForgeryToken()`, el `fetch()` lo manda en la cabecera y `Program.cs` declara `options.HeaderName` — sin esa última línea el atributo solo miraría los campos del formulario y rechazaría todo.

### D-25 · Las fechas de calendario se cuentan en la zona horaria del servidor
**Dónde:** `ServicioEntrenamientos.ObtenerRachas` → `e.Fecha.ToLocalTime()` y `DateTime.Now`; `DiarioController` → `DateTime.Today` (Fase 5.6)
**Qué pasa:** una racha y "lo que comí hoy" son conceptos de calendario — el día depende de la medianoche **del usuario**. Las fechas se guardan bien en UTC, pero para contar/agrupar días se usa la hora local del *servidor*, que es lo único que la app conoce hoy: nunca se le pregunta al usuario su zona horaria.
**Riesgo:** un usuario en otra zona puede ver su racha cortarse un día antes de tiempo, o registrar una comida "de hoy" que el servidor archiva en otra fecha. Con todos los usuarios y el servidor en la misma zona no se nota; se vuelve visible al desplegar en un servidor UTC (el caso típico en la nube) o con usuarios de otro país.
**Resolución:** Fase 10 — guardar la zona horaria en el perfil (o tomarla del navegador) y contar los días en la zona del usuario. `CalculadorRachas` ya está preparado (recibe las fechas y el "hoy" como parámetros) y el diario ya trabaja con `DateOnly`, así que solo cambia quién provee el "hoy".
**Estado:** ⬜ Abierta

### D-24 · El rate limiter es en memoria y no lee cabeceras de proxy
**Dónde:** `Program.cs` → `AddRateLimiter`, partición por `contexto.Connection.RemoteIpAddress`
**Qué pasa:** dos limitaciones del límite por IP agregado en la Fase 3. (1) El estado vive en la memoria del proceso: con más de una instancia, cada una lleva su propia cuenta y el límite efectivo se multiplica por la cantidad de nodos. (2) `RemoteIpAddress` detrás de un proxy o balanceador es la IP del proxy, así que todos los usuarios caerían en la misma partición y se bloquearían entre sí.
**Riesgo:** hoy ninguno — corre en una sola instancia y sin proxy. Se vuelve real en el despliegue a EC2, sobre todo si queda detrás de un balanceador; por eso queda como media y no como alta.
**Resolución:** Fase 10 (optimización y cierre, junto con el despliegue) — almacén compartido para el limitador y `UseForwardedHeaders` configurado con los proxies de confianza.
**Estado:** ⬜ Abierta

### D-30 · Halo blanco residual en un par de sprites de Koda
**Dónde:** `wwwroot/images/koda/koda-presentacion.png`, `koda-descanso.png` (y en menor medida otros)
**Qué pasa:** el recorte de fondo del sprite sheet (hecho fuera de la app) dejó un borde blanco tenue en algunos sprites. Se **disimula** con el glow neón azul y el flote (decisión de diseño, ADR-19), no se eliminó píxel a píxel para no arriesgar el pelaje blanco del lobo.
**Riesgo:** cosmético. Se nota solo si se mira de cerca y sin el glow.
**Resolución:** Fase 10 o cuando lleguen sprites limpios — *defringe* del borde (quitar píxeles casi blancos adyacentes a la transparencia) o reemplazo del asset.
**Estado:** ⬜ Abierta

### D-31 · PNGs `branding/*` sin uso tras cablear los sprites de Koda
**Dónde:** `wwwroot/images/branding/principal.png`, `logo.png`
**Qué pasa:** eran el lobo y el logo anteriores; la Fase 9 los reemplazó por los sprites de `images/koda/` y ya no los referencia ninguna vista.
**Riesgo:** ninguno; es peso muerto en el repo.
**Resolución:** Fase 10 — borrarlos en la limpieza final tras confirmar que nada los usa.
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
