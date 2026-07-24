# ADR-11: Validación en dos capas y defensa en profundidad del login

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 24/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-10 cerró el acceso (quién puede entrar y a qué datos). Este ADR cierra la entrada de datos: qué se acepta una vez adentro, y cuánto castigo aguanta la puerta antes de ceder. El roadmap no preveía un ADR para la Fase 3; se agrega porque la fase tomó dos decisiones que contradicen reglas escritas del proyecto y no deben quedar solo como comentarios en el código.

---

## Contexto

La Fase 3 arrancó con la deuda **D-04**: ni un solo atributo de validación en el proyecto. Se podía guardar peso `-50`, edad `500` o estatura `0`, y `PerfilController.GuardarPerfil` recibía cinco parámetros sueltos sin consultar `ModelState`.

El caso que mejor ilustra el problema es la estatura en `0`: la fórmula de Mifflin-St Jeor no divide por la estatura, la multiplica. Con `EstaturaCm = 0` el cálculo **no falla** — devuelve un número perfectamente formado y perfectamente falso. Es el peor tipo de bug: silencioso, plausible y sin traza.

Durante la fase se sumaron dos deudas detectadas al escribir el ADR-10:

- **D-21** — `IaCoachController.Consultar` era el único `POST` sin `[ValidateAntiForgeryToken]`.
- **D-22** — el login usaba `lockoutOnFailure: false`: nada limitaba cuántas contraseñas se podían probar.

---

## Decisión

### 1. La validación vive en dos capas, con responsabilidades distintas

| Capa | Dónde | Qué garantiza |
|---|---|---|
| Anotaciones | `UsuarioPerfil`, `RegistroProgreso`, ViewModels | Feedback inmediato y por campo a quien usa el formulario |
| Guardas | `CalculadorCaloricoService` | Que el cálculo sea imposible sobre datos absurdos, venga de donde venga |

La segunda capa no es redundante. Una anotación **solo actúa si alguien la evalúa**, y quien la evalúa es el pipeline de MVC. Un servicio llamado desde un job, desde otro servicio o desde un test no pasa por ese pipeline, y ahí la anotación no existe. La guarda sí.

`CalcularCaloriasDiarias` lanza `ArgumentOutOfRangeException` en vez de devolver un número inventado. Es deliberado: **fallar fuerte y visible es preferible a un resultado plausible y equivocado**, sobre todo en un dato que el usuario va a usar para comer.

### 2. Los rangos viven en una sola constante compartida

`FitnessCoach.Domain/Models/RangosPerfil.cs` centraliza los límites de `03-ESTANDARES.md` §1.2. Los consumen las dos entidades, los dos ViewModels, el request de la API y las guardas — seis lugares que antes habrían tenido los números copiados. Los mensajes de error usan los placeholders `{1}`/`{2}` de `RangeAttribute`, así que el texto sigue a la constante y no puede quedar mintiendo.

`System.ComponentModel.DataAnnotations` es parte de la biblioteca base de .NET, no de ASP.NET Core: usarla en `Domain` no agrega ningún `PackageReference` ni contradice la regla de dependencias del ADR-06.

### 3. Los formularios reciben ViewModels, no entidades

`GuardarPerfil` pasa de cinco parámetros sueltos a un `PerfilViewModel`. Se descartó recibir `UsuarioPerfil` directamente: el binder aceptaría un `Id` o un `IdentityUserId` enviados desde el navegador, reabriendo por la puerta de atrás justo lo que el ADR-10 cerró. El ViewModel expone solo los cinco campos que el usuario puede tocar.

### 4. Defensa en profundidad del login: dos mecanismos, no uno

| Mecanismo | Qué cuenta | Qué ataque frena |
|---|---|---|
| Bloqueo de Identity | Fallos **por cuenta** (5 → 15 min) | Fuerza bruta contra *una* cuenta |
| Rate limiter | Envíos **por IP** (10 por minuto) | *Password spraying* y alta de cuentas masiva |

El segundo existe porque el primero no alcanza. El bloqueo de Identity cuenta fallos por cuenta, así que un bot que prueba una única contraseña común contra miles de correos distintos **nunca acumula cinco fallos en ninguna** y pasaría entero. Ese ataque solo se ve desde el origen, no desde la cuenta.

Se usa el `RateLimiter` incorporado en .NET (`AddRateLimiter`), sin dependencias nuevas. Ventana fija con `QueueLimit = 0`: lo que excede se rechaza, no se encola. Se aplica a `Login` y a `Register` — este último, sin límite, es un generador gratuito de cuentas basura.

El rechazo se traduce según el consumidor: `/api` recibe `429` con `Retry-After`; el navegador vuelve a la pantalla de login con la explicación, porque un `429` crudo no le dice nada a una persona.

### 5. El bloqueo de cuenta se comunica de forma explícita

**Ésta es la decisión con costo.** El estándar §1.4 exige mensajes genéricos para no revelar qué correos están registrados. Un mensaje *"tu cuenta está bloqueada"* confirma que esa cuenta existe: es exactamente la enumeración de usuarios que la Fase 2 se ocupó de cerrar.

Se decidió igualmente mostrarlo, por prioridad de producto: una persona que no puede entrar y recibe *"correo o contraseña incorrectos"* seguirá intentando, agotará el bloqueo una y otra vez y concluirá que la app está rota. El costo real de la filtración es acotado — confirma la existencia de una cuenta que el atacante ya estaba atacando activamente —, mientras que el costo de la confusión lo paga cada usuario legítimo que se equivoque cinco veces.

Los mensajes de credenciales incorrectas **siguen siendo idénticos** entre "no existe el correo" y "la contraseña es incorrecta": la excepción es solo para el bloqueo, y se declara en `03-ESTANDARES.md` §1.4 para que no parezca un descuido.

Las cifras de los mensajes (5 intentos, 15 minutos) se leen de `IdentityOptions` inyectado, no están escritas en el texto: si la política cambia, el mensaje cambia con ella.

### 6. El token antiforgery viaja por cabecera en el chat

`[ValidateAntiForgeryToken]` solo busca el token en los campos del formulario. Como el chat del Lobo Coach postea JSON por `fetch()`, hizo falta declarar `options.HeaderName = "RequestVerificationToken"` en `Program.cs`; sin esa línea el atributo rechazaría todas las peticiones del chat y parecería que el chat se rompió. Cierra **D-21**.

---

## Alternativas Consideradas

### Alternativa 1: Validar solo con anotaciones
Confiar en que todo entra por MVC. **Se descarta:** deja el dominio dependiendo de que nadie llame nunca al servicio por otro camino, y el bug de la estatura `0` es precisamente el que sobrevive a esa suposición.

### Alternativa 2: Que `CalcularCaloriasDiarias` devuelva `null` o `0` ante datos inválidos
Evita la excepción y simplifica a los llamadores. **Se descarta:** un `0` calórico es un valor que el resto del sistema puede seguir usando sin notar nada, que es el problema original disfrazado. La excepción obliga a cada llamador a decidir explícitamente qué hacer, y hoy los dos que existen lo hacen: la vista muestra `—` y la API devuelve `422`.

### Alternativa 3: Poner las validaciones solo en los ViewModels, dejando el dominio limpio
Argumento válido: las `DataAnnotations` son una preocupación de presentación. **Se descarta:** el dominio también recibe datos desde el API y desde EF, y duplicar los rangos en cada ViewModel es exactamente la divergencia que `RangosPerfil` evita. La regla de dependencias se respeta igual, porque `DataAnnotations` es BCL.

### Alternativa 4: Rate limiting por cuenta en vez de por IP
Sería más justo con usuarios que comparten IP (una oficina, una universidad). **Se descarta:** duplicaría lo que el bloqueo de Identity ya hace y dejaría el *password spraying* sin cubrir, que es el hueco que motivó agregarlo.

### Alternativa 5: Mantener el mensaje genérico también para el bloqueo
Es lo que exige §1.4 al pie de la letra, y fue la implementación inicial. **Se descarta** por la razón del punto 5: el costo lo pagaba el usuario legítimo, y la información filtrada es de bajo valor para quien ya está atacando esa cuenta.

---

## Consecuencias

### Lo que gana el sistema
- **No entra un dato inválido por ninguna vía (D-04).** Formulario, API y llamada directa al servicio, los tres cubiertos.
- **El bug silencioso de la estatura `0` es ahora imposible**, y hay un test que lo fija.
- **El login resiste tanto la fuerza bruta como el spraying**, con dos mecanismos que cubren huecos distintos.
- **D-21 y D-22 cerradas** en la misma fase en que se detectaron.
- 66 pruebas en verde (32 nuevas), incluidos los límites exactos de cada rango.

### Lo que se asume o queda pendiente
- **El rate limiter es en memoria.** Con más de una instancia, cada una lleva su propia cuenta y el límite efectivo se multiplica por la cantidad de nodos. Al desplegar a EC2 hay que moverlo a un almacén compartido. Registrado como **D-24**.
- **Particiona por `RemoteIpAddress`.** Detrás de un proxy o balanceador esa es la IP del proxy, y todos los usuarios caerían en la misma partición; requiere `UseForwardedHeaders`. Parte de la misma **D-24**.
- **El mensaje de bloqueo revela la existencia de la cuenta**, por decisión explícita del punto 5.
- **Sigue sin haber confirmación de correo** (heredado del ADR-10): el rate limiter frena el alta masiva, pero no impide registrarse con una dirección ajena.
- Las validaciones de los controladores no tienen pruebas a nivel HTTP; lo cubierto son las anotaciones y las guardas. Requeriría `WebApplicationFactory`, pendiente heredado del ADR-08.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Ningún dato inválido llega a la base por ninguna ruta. D-04, D-21 y D-22 cerradas.
- ✅ Los rangos del estándar §1.2 viven en un solo lugar y los seis consumidores los comparten.
- ✅ `dotnet build` sin warnings; 66/66 pruebas en verde.
- ⏳ Pendiente (Fase 4): el tracker de progreso, que además construye la vista de Progreso inexistente (**D-23**) y unifica las fechas en UTC (**D-10**).
- ⏳ Deuda nueva de esta fase: **D-23** (vista de Progreso ausente) y **D-24** (rate limiter en memoria y sin cabeceras de proxy).
