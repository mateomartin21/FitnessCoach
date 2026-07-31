# ADR-10: Autenticación con ASP.NET Identity manteniendo el dominio libre de framework

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 24/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-09 conectó la persistencia real y dejó explícitamente anotado que la aplicación seguía operando sobre un usuario fijo (`Id = 1`) y con los endpoints REST abiertos. Este ADR cierra esa brecha: introduce el concepto de "mi cuenta" y blinda el acceso. Es la contraparte de seguridad del ADR-09.

---

## Contexto

Tras la Fase 1 los datos persistían, pero el sistema no tenía idea de **quién** era el dueño de esos datos. La revisión de deuda del 22/07/2026 documentó cinco problemas que en realidad son uno solo visto desde cinco ángulos:

- **D-02 — Usuario único hardcodeado.** Los cinco controladores MVC llamaban a `ObtenerPorId(1)`. No existía el concepto de cuenta: dos personas usando la app compartían y se sobrescribían el mismo perfil.
- **D-01 — Endpoints REST completamente abiertos.** Sin `[Authorize]` y con el id del dueño en la URL (`GET /api/usuarios/{id}`, `/api/usuarios/{id}/progreso`). IDOR total: bastaba cambiar un número para leer o escribir en la cuenta de otro. El hallazgo más grave del proyecto.
- **D-11 — `POST /api/usuarios` sin límite.** Cada llamada creaba un perfil nuevo, sin autenticación. Vector trivial de llenado de la base.
- **D-07 — `UseAuthentication()` ausente.** El pipeline llamaba a `UseAuthorization()` sin que hubiera identidad que evaluar. La línea existía y no hacía nada, dando falsa sensación de seguridad.
- **D-05 — Sin protección CSRF** en las acciones `[HttpPost]` que modifican estado.

El dilema de diseño de fondo: ASP.NET Identity es una pieza de framework grande y opinada (su propio `DbContext`, sus entidades, su `UserManager`). El ADR-06 estableció que `FitnessCoach.Domain` no referencia ningún paquete de infraestructura. La pregunta a resolver era **cómo introducir autenticación sin que Identity se filtre al dominio**.

---

## Decisión

### 1. El dominio guarda un identificador opaco, no una entidad de Identity

`UsuarioPerfil` gana una sola propiedad:

```csharp
public string? IdentityUserId { get; set; }   // a qué cuenta de Identity pertenece este perfil
```

Es un `string` común. **No** es una referencia de navegación a `ApplicationUser`, ni un tipo de Identity, ni obliga a `FitnessCoach.Domain` a referenciar `Microsoft.AspNetCore.Identity`. El dominio sabe que un perfil "pertenece a alguien identificado por esta cadena", y nada más. Si mañana se reemplaza Identity por Auth0, JWT o un proveedor externo, esta propiedad no cambia: solo cambia quién produce el valor.

Ésta es la decisión central del ADR y la razón de que el proyecto `Domain` siga sin dependencias de framework después de agregar autenticación.

### 2. La invariante "un usuario = un perfil" la garantiza la base de datos

```csharp
entity.HasIndex(u => u.IdentityUserId)
      .IsUnique()
      .HasFilter("[IdentityUserId] IS NOT NULL");
```

Índice único **filtrado**. El filtro es necesario porque SQL Server trata los `NULL` como valores iguales entre sí en un índice único: sin él, dos perfiles sin identidad asociada (los que pudieran quedar de la etapa anterior) violarían la restricción. Con el filtro, la regla aplica solo a los perfiles que sí tienen dueño.

La invariante no queda encomendada a que ningún código se equivoque: la impone el motor. Esto es, además, lo que cierra D-11 en su raíz — aunque alguien lograra llamar a un alta, no podría tener dos perfiles.

### 3. Identity comparte el `ApplicationDbContext` existente

`ApplicationDbContext` pasa a heredar de `IdentityDbContext<ApplicationUser>` en lugar de `DbContext`. Las tablas de Identity (`AspNetUsers`, `AspNetRoles`, …) conviven con `UsuariosPerfil` y `RegistrosProgreso` en la misma base, con una sola migración (`AgregaIdentity`) y una sola cadena de conexión.

Se descartó un `DbContext` separado para Identity (ver Alternativas): para un proyecto de este tamaño, dos contextos sobre la misma base introducen transacciones distribuidas y complejidad sin beneficio.

`ApplicationUser` vive en `Infrastructure/Identity/` — el único lugar del proyecto que conoce a Identity, junto con `Program.cs` y `AccountController`.

### 4. Un servicio de aplicación traduce identidad → perfil

Se crea `FitnessCoach.Application/Services/ServicioPerfilUsuario.cs` con tres operaciones:

| Operación | Qué hace |
|---|---|
| `ObtenerOCrear(identityUserId)` | Trae el perfil del usuario; si es su primera vez, le crea uno por defecto y lo guarda |
| `Obtener(identityUserId)` | Consulta sin crear (para lecturas que no deben tener efectos) |
| `Guardar(usuario)` | Delega en el repositorio |

Ambos métodos de lectura lanzan `ArgumentException` si el `identityUserId` viene vacío: sin identidad no hay dueño posible, y es preferible fallar fuerte a inventar un perfil huérfano.

`ObtenerOCrear` resuelve el problema del alta: no hay un momento explícito de "crear mi perfil" que alguien deba invocar (ni un endpoint que alguien pueda abusar). El perfil nace solo, la primera vez que el usuario entra a una pantalla que lo necesita.

El puerto `IRepositorioUsuario` gana `ObtenerPorIdentityUserId(string)`, implementado por **los dos** adaptadores (`RepositorioUsuarioSql` y `RepositorioUsuarioMemoria`), respetando la simetría que el ADR-09 decidió conservar.

Los controladores quedan con una sola forma de saber quién pide:

```csharp
private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
```

Ningún controlador vuelve a tocar `IRepositorioUsuario` directamente, y **ninguno acepta un id de usuario como parámetro**.

### 5. Las rutas de la API dejan de nombrar al usuario

El cambio que cierra D-01 no es solo agregar `[Authorize]`: es **quitar el id de la URL**.

| Antes | Ahora |
|---|---|
| `GET /api/usuarios/{id}` | `GET /api/perfil` |
| `GET /api/usuarios/{id}/calorias` | `GET /api/perfil/calorias` |
| `POST /api/usuarios` | *(eliminado)* |
| `GET /api/usuarios/{usuarioId}/progreso` | `GET /api/perfil/progreso` |
| `GET /api/usuarios/{usuarioId}/progreso/ultimo` | `GET /api/perfil/progreso/ultimo` |
| `POST /api/usuarios/{usuarioId}/progreso` | `POST /api/perfil/progreso` |

El razonamiento: mientras el id del dueño sea un parámetro de entrada, cada endpoint necesita recordar comprobar que el solicitante coincide con el dueño, y basta un olvido para reabrir el IDOR. Si el id **no existe como entrada**, no hay nada que comprobar ni que olvidar. El dueño se deriva de la identidad de la petición, que el cliente no controla.

Dos decisiones asociadas:

- **El `POST` de progreso ya no acepta la entidad de dominio completa.** Recibe un `NuevoRegistroRequest` con solo `PesoKg` y `Notas`; la `Fecha` la pone el servidor. Antes el cliente podía fijar cualquier fecha en el body.
- **Las respuestas no devuelven la entidad cruda.** `PerfilResponse` deja fuera `Id` e `IdentityUserId`: son internos y no aportan nada al consumidor.

### 6. Las rutas `/api` responden 401, no una redirección al login

La cookie de Identity, ante una petición no autenticada, responde `302` hacia `/Account/Login`. Ese comportamiento es correcto para un navegador y erróneo para un cliente de API, que recibiría un `200` con HTML de login en vez de un error. Se sobrescriben los eventos de la cookie:

```csharp
options.Events.OnRedirectToLogin = context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
};
```

Ídem `OnRedirectToAccessDenied` con `403`. Las rutas MVC conservan la redirección de siempre.

### 7. Vistas propias de autenticación, sin enumeración de usuarios

`AccountController` implementa Registro / Login / Logout con vistas propias en lugar del scaffolding de Identity, para que las pantallas se vean como el resto de la app (requisito explícito del roadmap).

Ante cualquier fallo de login el mensaje es idéntico — *"Correo o contraseña incorrectos."* —, y ante un fallo de registro también — *"No se pudo completar el registro."*. Un mensaje distinto según si el correo existe convierte el formulario en un oráculo para descubrir qué cuentas están registradas (estándar §1.4).

Las tres acciones `POST` llevan `[ValidateAntiForgeryToken]`, igual que `PerfilController.GuardarPerfil` y `ProgresoController.RegistrarPeso` — lo que cierra D-05.

### 8. Pruebas del servicio con un repositorio falso

`ServicioPerfilUsuarioTests` cubre trece casos con `RepositorioUsuarioFalso`, una implementación en memoria del puerto escrita a mano (sin librería de mocking, que el proyecto no usa). Verifica el alta del perfil nuevo, que un usuario existente no se duplique, que con varias cuentas en la base cada una reciba la suya, y que un `identityUserId` vacío falle.

Se prueba el **servicio de aplicación**, no los controladores ni el repositorio SQL: es la capa donde vive la regla "este perfil es de este usuario", y es probable sin base de datos, respetando la decisión del ADR-08 de que `FitnessCoach.Tests` no referencia `Infrastructure`.

---

## Alternativas Consideradas

### Alternativa 1: Que `UsuarioPerfil` navegue a `ApplicationUser`
Lo natural en EF Core sería una propiedad de navegación (`public ApplicationUser Usuario { get; set; }`) con su clave foránea. **Se descarta:** obligaría a `FitnessCoach.Domain` a referenciar `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, rompiendo la regla de dependencias del ADR-06 y convirtiendo el dominio en rehén del framework de autenticación. El `string` opaco da el 100% de la funcionalidad necesaria con 0% del acoplamiento; lo único que se pierde es que EF valide la integridad referencial hacia `AspNetUsers`, que es un precio bajo y consciente.

### Alternativa 2: Un `DbContext` separado para Identity
Aísla las tablas de autenticación de las del negocio. **Se descarta:** dos contextos sobre la misma base implican transacciones separadas y migraciones paralelas, complejidad que este proyecto no necesita. La separación conceptual ya está dada por el hecho de que el dominio no conoce Identity.

### Alternativa 3: Mantener el id en la URL y comprobar el dueño dentro de cada endpoint
Conservar `GET /api/usuarios/{id}` agregando una verificación `if (usuario.IdentityUserId != IdentityId) return Forbid();`. **Se descarta:** funciona, pero deja la seguridad dependiendo de que cada endpoint presente y futuro recuerde la comprobación. Quitar el id de la entrada elimina la clase entera de error en lugar de defenderse de ella caso por caso.

### Alternativa 4: JWT en vez de cookies
Más idiomático para una API pura. **Se descarta para esta fase:** la aplicación es principalmente MVC servido por servidor, donde la cookie es lo natural, y los endpoints REST hoy no tienen consumidores externos. Añadir JWT implicaría manejar dos esquemas de autenticación en paralelo. Queda anotado para cuando exista un cliente que lo justifique (ej. una app móvil, ya listada como idea fuera de alcance).

### Alternativa 5: Migrar los datos existentes al primer usuario registrado
Asociar el perfil huérfano con `Id = 1` a la primera cuenta creada. **Se descarta:** son datos de prueba de las fases anteriores, sin valor. El índice único filtrado los tolera (quedan con `IdentityUserId` nulo) y son inalcanzables desde la aplicación, porque ya no hay ninguna ruta que llegue a un perfil por su `Id`.

---

## Consecuencias

### Lo que gana el sistema
- **Existe el concepto de "mi cuenta" (D-02).** Cada usuario tiene su perfil, su historial y sus rutinas. Desaparece todo `ObtenerPorId(1)` del código.
- **El IDOR se cierra por diseño, no por vigilancia (D-01).** No hay id de usuario en ninguna entrada de la API.
- **`UseAuthentication()` está en el pipeline, antes de `UseAuthorization()` (D-07).** Los cinco controladores MVC llevan `[Authorize]`; los dos de API también.
- **No se pueden crear perfiles arbitrarios (D-11).** El endpoint desapareció y el índice único hace imposible el duplicado.
- **CSRF cubierto en las acciones POST de formularios (D-05).**
- El dominio sigue sin referencias a framework. Agregar autenticación no costó ni una dependencia en `FitnessCoach.Domain`.

### Lo que se asume o queda pendiente
- **`lockoutOnFailure: false` en el login.** No hay bloqueo tras intentos fallidos: nada frena un ataque de fuerza bruta contra una contraseña. Registrado como **D-22**.
- **`IaCoachController.Consultar` es un `POST` sin `[ValidateAntiForgeryToken]`.** Se consume por `fetch()` con JSON, lo que en la práctica lo protege (un `Content-Type: application/json` cross-origin dispara preflight CORS, que falla), pero es una excepción a la regla del estándar §5 y no debería quedar implícita. Registrado como **D-21**.
- **Las políticas de contraseña son laxas** (`RequiredLength = 6`, sin requisito de caracteres no alfanuméricos), heredadas de la configuración inicial. Suficiente para el contexto académico, insuficiente para producción.
- **Sin confirmación de correo** (`RequireConfirmedAccount = false`): cualquiera se registra con una dirección que no le pertenece. Requiere un servicio de envío de correo, fuera del alcance de la fase.
- **Los controladores de API no tienen pruebas automatizadas.** Lo probado es el servicio de aplicación. Verificar el `[Authorize]` y el aislamiento a nivel HTTP requeriría pruebas de integración con `WebApplicationFactory`, pendiente heredado del ADR-08.
- Quedan en la base los perfiles huérfanos de las fases anteriores, inalcanzables pero presentes.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Autenticación real con ASP.NET Identity: registro, login y logout con vistas propias.
- ✅ Cada usuario ve y modifica únicamente sus datos, en las vistas MVC y en la API.
- ✅ D-01, D-02, D-05, D-07 y D-11 cerradas. La Fase 2 cumple su alcance completo.
- ✅ `dotnet build` sin warnings; 34/34 pruebas en verde (13 nuevas sobre `ServicioPerfilUsuario`).
- ⏳ Pendiente (Fase 3): validación de entrada en todos los modelos y controladores (D-04) — hoy sigue siendo posible guardar un peso negativo o una estatura de 0 desde el formulario de perfil.
- ⏳ Deuda nueva detectada en esta fase: D-21 (CSRF en `IaCoachController`) y D-22 (sin bloqueo por intentos fallidos de login).
