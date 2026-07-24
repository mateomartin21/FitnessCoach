# ADR-09: Cierre de la deuda de persistencia — adaptador SQL real para `IRepositorioUsuario`

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 23/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** Este ADR cierra formalmente una deuda que el ADR-07 dejó abierta. El ADR-07 declaró la persistencia con EF Core y SQL Server como "resuelta" y dejó modelados el `ApplicationDbContext` y la migración `InitialCreate`, pero en la práctica `Program.cs` seguía registrando `RepositorioUsuarioMemoria` como `Singleton`: ningún repositorio consumía el `DbContext`. Es decir, la infraestructura de persistencia existía pero no estaba conectada, y los datos se perdían en cada reinicio del servidor. Este ADR conecta esa infraestructura y documenta honestamente la brecha entre lo que el ADR-07 dio por hecho y lo que realmente corría.

---

## Contexto

El ADR-07 formalizó la migración a persistencia real (EF Core + SQL Server LocalDB) y la seguridad de la API key de Gemini. Dejó bien modelados `ApplicationDbContext` (con el `HasConversion` del `ObjetivoActual` y la colección *owned* `HistorialProgreso`) y la migración `InitialCreate`.

El problema, detectado en la revisión de deuda técnica del 22/07/2026 (deudas **D-03** y **D-06**):

- **D-03 — La base de datos existe pero no se usa.** `Program.cs` registraba el puerto `IRepositorioUsuario` contra `RepositorioUsuarioMemoria` (un `List<UsuarioPerfil>` en RAM). El `ApplicationDbContext` estaba registrado, pero ningún repositorio lo consumía. Consecuencia: **todos los datos se perdían en cada reinicio del servidor**, a pesar de que el ADR-07 se leía como si la persistencia ya funcionara.
- **D-06 — Repositorio singleton con estado mutable no sincronizado.** `RepositorioUsuarioMemoria` estaba registrado como `Singleton` y compartía una `List<>` mutable entre todas las peticiones sin ningún bloqueo. La asignación de ID (`usuario.Id = _usuarios.Count + 1`) no es atómica: dos peticiones simultáneas podían generar el mismo ID o corromper la lista.

Esta es exactamente la clase de brecha que el índice de contexto advierte en su regla de mantenimiento: *"nada se documenta como hecho si no está verificado"*. El ADR-07 dio la persistencia por resuelta sin que estuviera conectada en tiempo de ejecución.

---

## Decisión

### 1. Segundo adaptador del puerto: `RepositorioUsuarioSql`

Se crea `FitnessCoach.Infrastructure/Repositories/RepositorioUsuarioSql.cs`, que implementa `IRepositorioUsuario` sobre `ApplicationDbContext`. Es un segundo adaptador del mismo puerto, sin cambiar la interfaz ni tocar el dominio ni los controladores — arquitectura hexagonal en su forma más pura: **un puerto, dos adaptadores intercambiables**.

Se mantuvo la firma síncrona del puerto (`ObtenerPorId`, `Guardar`) para no propagar cambios a los controladores. Migrar a una API asíncrona (`async`/`await` con `SaveChangesAsync`) queda anotado como refactor posible a futuro, fuera del alcance de esta fase.

El método `Guardar` distingue tres casos, porque los controladores existentes usan el repositorio de formas distintas:

| Caso | Cuándo ocurre | Comportamiento |
|---|---|---|
| Entidad ya rastreada | `ProgresoController` hace `ObtenerPorId(1)`, agrega un registro al historial y llama a `Guardar` con esa misma instancia | EF ya detectó el cambio; basta `SaveChanges()` |
| Alta (`Id == 0`) | `PerfilController.Index` crea un perfil demo nuevo | `Add()` + `SaveChanges()`; la identidad de SQL Server asigna el `Id` |
| Entidad *detached* con `Id` existente | `PerfilController.GuardarPerfil` arma un `UsuarioPerfil` nuevo con `Id = 1` | Se carga el existente y se copian solo los valores escalares con `Entry(existente).CurrentValues.SetValues(usuario)` — **no** se toca `HistorialProgreso`, así que el historial ya persistido se conserva |

El tercer caso es el más delicado: `SetValues` copia únicamente propiedades escalares (`Nombre`, `Edad`, `PesoKg`, `EstaturaCm`, `ObjetivoActual`), no la navegación *owned* del historial. Sin ese cuidado, editar el perfil desde el formulario habría borrado el historial de progreso.

### 2. Registro como `Scoped` en `Program.cs`

```diff
- builder.Services.AddSingleton<IRepositorioUsuario, RepositorioUsuarioMemoria>();
+ builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuarioSql>();
```

El cambio de `Singleton` a `Scoped` no es cosmético: **resuelve D-06**. `Scoped` da una instancia por petición HTTP, que es además el ciclo de vida que EF Core espera para su `DbContext` (registrado también como `Scoped` vía `AddDbContext`). La condición de carrera del singleton con estado mutable desaparece por completo, porque ya no hay una lista compartida entre peticiones — el estado vive en SQL Server, con su propia gestión de concurrencia e identidad.

### 3. Destino de `RepositorioUsuarioMemoria`: se conserva

Se decidió **conservar** `RepositorioUsuarioMemoria` en lugar de eliminarlo. Razones:

- Es una demostración limpia del patrón Ports & Adapters: dos adaptadores del mismo puerto, intercambiables cambiando una sola línea en `Program.cs`. Tiene valor pedagógico y de sustentación.
- Permite ejecutar la aplicación sin una base de datos configurada (útil para pruebas manuales rápidas o para levantar la app en un entorno sin LocalDB).
- No tiene costo: no se instancia mientras `Program.cs` registre el adaptador SQL, y no introduce dependencias nuevas.

### 4. Primer ejercicio real del Factory Method sobre la base de datos

Esta fase es el primer momento en que el `HasConversion` del `ObjetivoActual` se ejercita de verdad contra SQL Server. `ObjetivoFitness` es una clase abstracta sin datos propios (patrón Strategy): se persiste el nombre del tipo concreto en la columna `ObjetivoActualTipo` y se reconstruye al leer vía `ObjetivoFitnessFactory` (Factory Method, ADR-06). Guardar un perfil, reiniciar y recuperarlo comprueba que el Strategy sobrevive el viaje de ida y vuelta a la base de datos.

---

## Alternativas Consideradas

### Alternativa 1: Hacer asíncrona la interfaz del repositorio
Cambiar `IRepositorioUsuario` a `Task<UsuarioPerfil?> ObtenerPorIdAsync(int)` / `Task GuardarAsync(...)`. Se descarta para esta fase: obligaría a modificar los cinco controladores que consumen el puerto y ampliaría el alcance más allá de "conectar la persistencia". Queda anotado como refactor futuro.

### Alternativa 2: Eliminar `RepositorioUsuarioMemoria`
Dejar un único adaptador. Se descarta por las razones del punto 3 de la Decisión: el doble adaptador es barato de mantener y valioso como evidencia del patrón.

### Alternativa 3: Usar `Update()` en vez de `SetValues()` para el caso *detached*
`_context.UsuariosPerfil.Update(usuario)` marca todo el grafo como modificado, incluida la colección *owned* `HistorialProgreso` — que en el caso de `GuardarPerfil` llega vacía. Eso habría intentado borrar los registros de progreso existentes. Se descarta por incorrecto; `SetValues` sobre los escalares es la operación que preserva el historial.

---

## Consecuencias

### Lo que gana el sistema
- **Los datos sobreviven a un reinicio del servidor.** Se resuelve D-03: crear un perfil, registrar progreso, reiniciar `dotnet run` y encontrar todo intacto.
- **Desaparece la condición de carrera del singleton (D-06).** El estado ya no vive en una `List<>` compartida, sino en SQL Server con `Scoped` por petición.
- El `HasConversion` del `ObjetivoActual` queda validado de extremo a extremo por primera vez.
- El dominio y los controladores no cambiaron: la migración de adaptador confirma que la abstracción del puerto estaba bien diseñada.

### Lo que se asume o queda pendiente
- `RepositorioUsuarioSql` sigue sin cobertura de pruebas automatizadas: `FitnessCoach.Tests` no referencia `Infrastructure` por decisión del ADR-08. Probarlo requeriría un proyecto de integración separado (SQLite in-memory o LocalDB), pendiente ya anotado en el ADR-08.
- La aplicación sigue operando sobre un usuario fijo (`Id = 1`) hardcodeado en los controladores (deuda **D-02**). La persistencia ahora es real, pero todavía no existe el concepto de "mi cuenta": eso lo resuelve la Fase 2 (Identity, multiusuario y blindaje), que este ADR deja como siguiente paso directo.
- Los endpoints REST siguen abiertos (D-01) y sin CSRF (D-05); también corresponden a la Fase 2.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Arquitectura hexagonal multiproyecto (ADR-06), persistencia real conectada (este ADR) y suite de pruebas + CI (ADR-08) vigentes.
- ✅ `RepositorioUsuarioSql` conectado como `Scoped`: los datos persisten en SQL Server y sobreviven a reinicios. D-03 y D-06 resueltas.
- ✅ `RepositorioUsuarioMemoria` conservado como segundo adaptador del puerto.
- ✅ `dotnet build` sin warnings; 21/21 pruebas en verde.
- ⏳ Pendiente (Fase 2): autenticación con ASP.NET Identity, multiusuario, eliminación del `Id = 1` hardcodeado, `[Authorize]`, CSRF y cierre de la enumeración de usuarios (D-01, D-02, D-05, D-07, D-11).
