# 03 — Estándares de calidad

> **El estándar declarado:** *"que mis compañeros al usar mi app no puedan encontrar errores de lógica"*. Este documento convierte esa intención en una lista verificable. Nada de código nuevo se da por terminado si no cumple la sección "Definition of Done".

---

## 1. Seguridad — los cinco vectores

Cuando alguien intenta romper una app web como esta, va por estos cinco huecos. Los cuatro primeros existían en el proyecto: la Fase 2 cerró IDOR, CSRF y enumeración de usuarios, y la Fase 3 cerró la validación de entrada.

### 1.1 IDOR — *Insecure Direct Object Reference*

**Qué es:** cambiar un número en la URL para ver datos ajenos.

```
GET /api/usuarios/1   ← mi perfil
GET /api/usuarios/2   ← el perfil de otro. ¿Me lo devuelve?
```

**Estado actual:** ✅ resuelto en la Fase 2 (ADR-10). Las rutas de arriba ya no existen: la API cuelga de `/api/perfil` y `/api/perfil/progreso`, sin ningún id de usuario en la URL.

**Regla:** un endpoint **nunca** confía en el ID que viene de la petición para decidir a quién pertenece un recurso. El dueño se determina desde la identidad autenticada (`User.FindFirstValue(ClaimTypes.NameIdentifier)`).

**Preferencia, cuando se pueda:** que el id del dueño **no sea un parámetro de entrada en absoluto**. Mientras exista, cada endpoint nuevo tiene que acordarse de comprobar que el solicitante coincide, y basta un olvido para reabrir el agujero. Si no hay id que recibir, no hay nada que olvidar. Cuando la ruta sí necesite un id (por ejemplo, un registro concreto del historial en la Fase 4), se verifica la pertenencia y se responde `404` si falla, nunca `403` — ver 1.4.

### 1.2 Validación de entrada

**Qué es:** confiar en que el formulario mande datos razonables.

**Estado actual:** ✅ resuelto en la Fase 3 (ADR-11). Los rangos de la tabla de abajo viven en `RangosPerfil` (proyecto `Domain`) y los comparten las entidades, los ViewModels y el request de la API. Con estatura `0` el cálculo ya no devuelve un número falso: lanza `ArgumentOutOfRangeException`.

**Reglas:**
- Todo modelo que llegue desde el exterior lleva anotaciones (`[Required]`, `[Range]`, `[StringLength]`).
- Todo controlador que reciba datos verifica `ModelState.IsValid` **antes** de tocar nada.
- Los rangos se definen con criterio de dominio, no arbitrarios:

| Campo | Rango razonable | Por qué |
|-------|----------------|---------|
| `Edad` | 13–100 | Menores de 13 no deberían usar la app sin supervisión; >100 es dato erróneo |
| `PesoKg` | 30–300 | Fuera de eso es error de captura |
| `EstaturaCm` | 100–250 | Evita división/multiplicación con valores absurdos |
| `Nombre` | 2–100 caracteres, requerido | — |
| `Notas` (progreso) | máx. 500 caracteres | Evita abuso de almacenamiento |

- **La validación va en dos capas:** anotaciones en el modelo (rápida, para el usuario) *y* la regla de negocio en el dominio cuando aplique. Las anotaciones se pueden saltar llamando al API directamente si algún día se desactiva `[ApiController]`.

### 1.3 CSRF — *Cross-Site Request Forgery*

**Qué es:** una página maliciosa hace que tu navegador, ya autenticado, envíe un POST a la app sin que lo sepas.

**Estado actual:** ✅ resuelto en la Fase 2 (ADR-10) para los formularios: `PerfilController.GuardarPerfil`, `ProgresoController.RegistrarPeso` y las tres acciones de `AccountController` llevan `[ValidateAntiForgeryToken]`. Queda una excepción: `IaCoachController.Consultar`, que se llama por `fetch()` y no envía el token (deuda **D-21**, Fase 3).

**Regla:** **toda** acción MVC que modifique estado lleva `[ValidateAntiForgeryToken]`, y su formulario Razor usa `asp-action` (que inyecta el token automáticamente). Los endpoints REST con `[ApiController]` consumidos por `fetch()` se protegen con la política de cookies `SameSite=Lax` + verificación de origen.

### 1.4 Enumeración de usuarios

**Qué es:** el login que responde *"ese correo no está registrado"* le regala al atacante la lista de quién sí lo está.

**Regla:**
- El login responde **siempre el mismo mensaje genérico** ante credenciales incorrectas: *"Correo o contraseña incorrectos."* Nunca distingue cuál de los dos falló.
- El registro no revela si un correo ya existe de forma explotable.
- Un recurso ajeno responde `404`, no `403`. Un `403` confirma que el recurso existe.

**Excepción declarada (Fase 3, ADR-11):** el **bloqueo de cuenta sí se comunica explícitamente** — *"Tu cuenta quedó bloqueada temporalmente…"* —, aunque eso confirme que la cuenta existe. Se aceptó el costo porque el mensaje genérico dejaba al usuario legítimo intentando una y otra vez sin entender nada, y la información que se filtra es de bajo valor: confirma una cuenta que el atacante ya estaba atacando. La distinción entre "correo inexistente" y "contraseña incorrecta" **sigue sin revelarse**.

### 1.5 Autorización a nivel de dato

**Qué es:** `[Authorize]` solo garantiza que *alguien* inició sesión. No garantiza que ese alguien tenga derecho sobre *ese* dato.

**Regla:** `[Authorize]` es el piso, no el techo. Toda operación sobre un `UsuarioPerfil` o un `RegistroProgreso` verifica pertenencia contra el usuario autenticado. En la práctica esto significa que el repositorio expone métodos del tipo `ObtenerPorIdentityUserId(string)`, y que **el `Id` numérico del perfil nunca viaja en la URL como forma de seleccionar de quién son los datos**.

---

## 2. Robustez y lógica

### Concurrencia

`RepositorioUsuarioMemoria` está registrado como **singleton** y usa un `List<UsuarioPerfil>` mutable sin sincronización. Dos peticiones simultáneas pueden corromper la lista o duplicar IDs (`usuario.Id = _usuarios.Count + 1` no es atómico). Es un bug real, no teórico — dos pestañas guardando a la vez lo reproducen.

**Regla:** ningún servicio con estado mutable se registra como singleton. Los repositorios respaldados por `DbContext` van como **`Scoped`** (una instancia por petición), que es además lo que EF Core espera.

### Invariantes garantizadas por la base de datos

Una regla de negocio importante no se defiende solo con un `if` en el controlador — un `if` pierde ante dos peticiones concurrentes.

**Regla:** las invariantes duras se declaran en el esquema. Ejemplo: "un usuario de Identity tiene exactamente un perfil" → índice único sobre `IdentityUserId`, no un `if (yaExiste)`.

### Manejo de fechas

Hoy hay inconsistencia real: `ProgresoController` usa `DateTime.Now` (hora local del servidor) y `ProgresoApiController` usa `DateTime.UtcNow`. Dos caminos que escriben la misma tabla con criterios distintos → el historial se ordena mal.

**Regla:** **todo se guarda en UTC** (`DateTime.UtcNow`). La conversión a hora local se hace solo al mostrar, en la vista.

### Errores que no se disfrazan de éxito

`GeminiCoachService` hoy captura los errores y **devuelve el mensaje de error como si fuera texto normal del coach**. La UI no puede distinguir "el Lobo Coach te aconseja X" de "falló la conexión". Eso impide construir un fallback encima.

**Regla:** una capa de infraestructura señala el fallo de forma que el llamador pueda reaccionar (excepción propia, o un tipo de resultado explícito). Nunca devuelve un mensaje de error como si fuera un resultado válido.

### Nada de `catch (Exception) { }` mudo

Si se captura, o se maneja de verdad, o se registra con `ILogger` y se relanza. Un `catch` vacío convierte un bug en un misterio.

---

## 3. Pruebas

**Estado actual:** 95 pruebas xUnit, todas en verde, corriendo en CI.

### Reglas

- **Toda lógica de negocio nueva en `Domain` o `Application` llega con sus pruebas en el mismo commit.** No "después".
- **Nombre del test:** `Metodo_Escenario_ResultadoEsperado`
  → `CalcularCaloriasDiarias_ConObjetivoPerderPeso_AplicaMultiplicador085`
- **Estructura:** `// Arrange` / `// Act` / `// Assert`, con los comentarios visibles.
- **Los casos límite son obligatorios**, no opcionales: `null`, cero, negativo, valor fuera de rango, colección vacía.
- **Test doubles escritos a mano** para aislar (ej. `EstrategiaFalsa`). No se agrega una librería de mocking mientras los dobles a mano sigan siendo simples de mantener (decisión del ADR-08).
- **Una prueba no debe depender del contenido de datos que puede cambiar.** Los tests del Decorator usan una estrategia falsa justamente para no romperse cuando se agreguen ejercicios reales a `EstrategiaGanarMusculo` — algo que va a pasar en la Fase 5.
- **Nada de pruebas que necesiten base de datos, red o API key** en `FitnessCoach.Tests`. Eso rompería el pipeline.

### Antes de cada push

```powershell
dotnet build FitnessCoach.slnx
dotnet test FitnessCoach.Tests/FitnessCoach.Tests.csproj
```

Ambos en verde. El pipeline lo va a verificar de todos modos, pero enterarse localmente es más rápido.

---

## 4. Nombres y organización

- El nombre del archivo **es** el nombre del tipo. Sin excepciones.
- Interfaces con prefijo `I`, y el archivo se llama igual (`IGeneradorRutinas` → `IGeneradorRutinas.cs`).
- Sin archivos `Class1.cs` ni sobras de plantilla.
- Sin código comentado "por si acaso" — para eso está el historial de Git.
- Las carpetas reflejan el concepto, no el tipo técnico (`Patterns/Strategy/`, no `Interfaces/`).

---

## 5. Commits y ramas

- **Una rama por fase** del roadmap: `fase-2/identity-login`, `fase-4/tracker`, etc.
- **Commits en imperativo y en español**, con prefijo de tipo:
  - `feat:` funcionalidad nueva
  - `fix:` corrección de bug
  - `refactor:` cambio interno sin alterar comportamiento
  - `test:` pruebas
  - `docs:` documentación / ADRs
  - `chore:` mantenimiento
- **Un commit = un cambio coherente.** El historial debe poder leerse como la narración de la fase; es literalmente un criterio de evaluación de las actividades del curso.
- **PR por fase**, contra la rama base, con el check de CI en verde antes de mergear.

---

## 6. Definition of Done

Una tarea está terminada solo cuando **todo** esto se cumple:

- [ ] Compila sin errores ni warnings nuevos
- [ ] `dotnet test` en verde, con pruebas **nuevas** que cubren la lógica agregada
- [ ] Respeta la regla de dependencias de `02-ARQUITECTURA.md` (nada de framework en `Domain`)
- [ ] Entradas validadas (anotaciones + `ModelState.IsValid`)
- [ ] Endpoints protegidos: `[Authorize]` + verificación de pertenencia del dato
- [ ] Acciones POST con `[ValidateAntiForgeryToken]`
- [ ] Sin `Id` hardcodeado, sin datos de prueba quemados en el código
- [ ] Fechas en UTC
- [ ] Los errores se propagan; no se disfrazan de resultado válido
- [ ] Pipeline de CI en verde en el PR
- [ ] `04-DEUDA-TECNICA.md` actualizado si se resolvió o se descubrió deuda
- [ ] ADR escrito si hubo una decisión de arquitectura no obvia

---

## 7. Prueba de fuego

Antes de cerrar una fase, intenta romperla tú mismo como lo haría un compañero:

1. Abre dos sesiones con usuarios distintos. ¿El usuario A ve algo del usuario B?
2. Cambia el número en cualquier URL. ¿Se filtra información?
3. Manda el formulario con peso `-1`, edad `0`, nombre vacío, y con 5000 caracteres en un campo de texto.
4. Manda un POST al API sin haber iniciado sesión.
5. Guarda dos veces seguidas, rápido, desde dos pestañas.
6. Desconecta internet y usa el Lobo Coach. ¿Se cae la app o degrada con gracia?
7. Reinicia el servidor. ¿Siguen ahí los datos?

Si alguna de estas siete rompe algo, la fase no está terminada.
