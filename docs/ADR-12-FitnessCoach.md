# ADR-12: El tracker como historial de hechos, con reglas en la capa de aplicación

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 24/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-10 definió de quién son los datos y el ADR-11 qué datos se aceptan. Este ADR define **qué se guarda de lo que el usuario hace**: el modelo de dominio del tracker y dónde viven sus reglas. Es el primer ADR de la serie que agrega entidades nuevas en vez de blindar las existentes.

---

## Contexto

Tras la Fase 3 el proyecto guardaba pesos y calculaba calorías, pero no registraba **actividad**. La Fase 4 arrancó con tres deudas y un descubrimiento:

- **D-12 — `RegistroProgreso` sin identidad propia.** El `Id` existía solo como *shadow property* de EF: la base sabía distinguir registros, el dominio no. Sin identidad no hay forma de decir "edita *este* registro".
- **D-10 — Fechas inconsistentes.** `ProgresoController` usaba `DateTime.Now` y `ProgresoApiController` usaba `DateTime.UtcNow`. Dos caminos escribiendo la misma colección con criterios horarios distintos: el historial se ordenaba mal al mezclarlos.
- **D-23 — La vista de Progreso no existía.** Detectada al empezar esta fase: `ProgresoController.Index` hacía `return View(historial)` contra un archivo ausente, así que `/Progreso` lanzaba una excepción. Pasó inadvertido durante todo el proyecto porque el menú no enlazaba esa pantalla — nadie llegaba nunca.

El descubrimiento es que las tres son la misma deuda vista desde ángulos distintos: **el progreso estaba modelado como un apéndice del perfil, no como un historial de hechos con entidad propia.**

---

## Decisión

### 1. Los registros son hechos con identidad

`RegistroProgreso.Id` pasa a ser una propiedad del dominio y el mapeo usa `HasKey(r => r.Id)` en vez de la *shadow property*. **No hubo cambio de esquema**: la columna ya existía, lo único que cambió es que ahora el modelo la conoce. Eso confirma que D-12 era una deuda de modelado, no de base de datos.

Se agrega `EntrenamientoCompletado` como segunda colección *owned* del perfil, con la misma forma: identidad propia, fecha en UTC, y datos del hecho.

`EntrenamientoCompletado` guarda **el nombre de la rutina como texto**, no una referencia a `Rutina`. Dos razones:

- Las rutinas se generan al vuelo desde las estrategias (ADR-06) y no tienen identidad persistida. No hay a qué apuntar.
- Aunque la tuvieran, un registro histórico no debe seguir cambiando cuando cambia el catálogo: el entrenamiento del martes tiene que seguir diciendo lo que decía el martes.

Cuando la Fase 5 cree la entidad `Ejercicio`, este modelo no se invalida: se le podrá agregar una referencia opcional sin perder los registros ya hechos.

### 2. Todas las fechas se guardan en UTC y se convierten al mostrar

Se unifica en `DateTime.UtcNow` (D-10). Pero el cambio obvio no alcanzaba: SQL Server guarda `datetime2` **sin zona horaria**, así que al leer la fecha vuelve con `Kind = Unspecified` y un `ToLocalTime()` posterior **no convierte nada** — mostraría UTC creyendo que es hora local.

Por eso ambas colecciones llevan un converter que marca la fecha como UTC al materializarla:

```csharp
progreso.Property(r => r.Fecha)
    .HasConversion(
        fecha => fecha,
        fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
```

Sin esa pieza, D-10 quedaría "cerrada" en el código y rota en pantalla. Es el tipo de arreglo que se ve completo en el diff y no lo está.

### 3. Las reglas del historial viven en la capa de aplicación

`ServicioProgreso` y `ServicioEntrenamientos` concentran las decisiones que no son ni de presentación ni de persistencia:

| Regla | Por qué |
|---|---|
| El peso del perfil sigue al registro más reciente | Editar el último registro debe reflejarse en el perfil; editar uno viejo, no |
| Borrar el último registro que queda **conserva** el peso anterior | Dejarlo en 0 pondría el perfil fuera del rango válido y haría lanzar al cálculo calórico — el bug que cerró el ADR-11, reintroducido por la puerta de atrás |
| Toda operación arranca del perfil del usuario autenticado | Un registro ajeno no aparece: no hay id que manipular (misma idea del ADR-10, un nivel más adentro) |

La primera versión de esta fase puso estas reglas dentro del controlador. Se corrigió antes de commitear: ahí eran imposibles de probar sin levantar HTTP, y el controlador dejaba de ser un traductor de peticiones para volverse dueño de decisiones del negocio.

El resultado directo de ese movimiento son las 20 pruebas nuevas de ambos servicios, incluidas las tres que verifican que una cuenta no puede editar, borrar ni leer los registros de otra.

### 4. Las rachas se calculan con una función pura

`CalculadorRachas` es estático, sin estado y **sin reloj propio**: el "hoy" entra por parámetro.

```csharp
public static Rachas Calcular(IEnumerable<DateTime> fechasEntrenadas, DateOnly hoy)
```

Recibir el "hoy" en vez de leer `DateTime.Now` adentro es lo que permite fijarlo en las pruebas. Un cálculo de fechas que consulta el reloj del sistema produce tests que pasan hoy y fallan en Año Nuevo, o que dependen de la hora a la que se corran.

Dos reglas de producto quedaron decididas acá y fijadas por pruebas:

- **Entrenar ayer mantiene la racha viva.** Si se cortara a la medianoche, quien entrena de tarde vería su racha en cero cada mañana antes de entrenar.
- **Varios entrenamientos el mismo día cuentan como un solo día.** La racha mide constancia, no volumen.

### 5. Chart.js servido localmente, sin CDN

La gráfica de evolución usa Chart.js 4.4.7 copiado a `wwwroot/lib/`, con el mismo layout que jQuery y Bootstrap (que ya estaban así). La app sigue funcionando sin internet y la navegación de los usuarios no se le reporta a un tercero.

El eje Y **no empieza en cero**, contra la recomendación habitual: la variación de peso de una persona se mide en pocos kilos sobre una base de 60–90, y un eje desde 0 aplasta la línea hasta volverla ilegible. Los datos viajan en atributos `data-*` serializados con `@Json.Serialize`, no interpolados dentro del `<script>`.

### 6. Los récords por ejercicio se mueven a la Fase 5

Era un entregable de esta fase. Se pospone porque **depende de que los ejercicios tengan identidad**, y hoy están hardcodeados dentro de cada Strategy sin más identificador que su nombre. Usar el nombre como clave funcionaría hoy y sería frágil: renombrar un ejercicio perdería el récord, y la Fase 5 tendría que migrar esos datos a la entidad nueva. Se hace cuando exista `Ejercicio` como concepto de primera clase.

---

## Alternativas Consideradas

### Alternativa 1: `EntrenamientoCompletado` con referencia a `Rutina`
Sería lo natural en un modelo relacional. **Se descarta:** las rutinas no se persisten, y un historial que cambia cuando cambia el catálogo no es un historial.

### Alternativa 2: Guardar las fechas en hora local
Evita conversiones y es más simple de leer en la base. **Se descarta:** es exactamente la causa de D-10. Con hora local, mover el servidor de zona corrompe el orden de todo el historial ya guardado, y no hay forma de saber en qué zona se escribió cada fila.

### Alternativa 3: Dejar las reglas del historial en el controlador
Menos archivos y menos indirección. **Se descarta** por lo dicho en el punto 3: sin poder probarlas, reglas como "el peso sigue al registro más reciente" se rompen en el primer refactor y nadie se entera.

### Alternativa 4: Calcular las rachas en una consulta SQL
Más eficiente con muchos registros. **Se descarta:** ataría una regla de producto al motor de base de datos, la volvería imposible de probar sin BD (contra el ADR-08) y la optimización no hace falta a esta escala. `CalculadorRachas` opera sobre listas en memoria de decenas de elementos.

### Alternativa 5: Edición en línea en la tabla del historial
Más rápido para el usuario, sin navegar a otra pantalla. **Se descarta para esta fase:** manejar los errores de validación por fila complica la vista, y el patrón de vista separada es consistente con el resto de la app. Queda como mejora de UX para la Fase 9.

---

## Consecuencias

### Lo que gana el sistema
- **La pantalla de Progreso existe y es alcanzable desde el menú (D-23).** Se agregó el enlace, que era la razón de que el bug sobreviviera tanto tiempo.
- **Los registros se pueden editar y borrar (D-12)**, con aislamiento entre cuentas verificado por pruebas.
- **Las fechas son coherentes en toda la app (D-10)**, guardadas en UTC y mostradas en local.
- **El tracker registra actividad, no solo peso**: entrenamientos, racha actual, mejor racha y gráfica de evolución.
- 95 pruebas en verde (29 nuevas en esta fase).

### Lo que se asume o queda pendiente
- **Las rachas se cuentan en la zona horaria del servidor (D-25).** La app nunca le pregunta al usuario la suya. Se nota al desplegar en un servidor UTC o con usuarios de otro país. `CalculadorRachas` ya recibe las fechas y el "hoy" por parámetro, así que arreglarlo no lo toca.
- **Los récords por ejercicio no existen todavía** (movidos a la Fase 5, punto 6).
- **La API REST no expone entrenamientos ni edición/borrado de registros.** Solo la vista MVC opera sobre ellos; los endpoints quedaron en lo que definió el ADR-10. Registrado como **D-26**.
- Los controladores siguen sin pruebas a nivel HTTP; lo cubierto son los servicios. Pendiente heredado del ADR-08.
- `Ejercicio` sigue viviendo dentro de `Entrenamiento.cs`, un archivo cuyo nombre no coincide con el tipo — el mismo problema que la Fase 0 corrigió en otros dos archivos (D-16) y que este pasó por alto. Lo absorbe la Fase 5, que reescribe ese modelo entero.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Tracker funcional: historial de peso editable, entrenamientos, rachas y gráfica de evolución.
- ✅ D-10, D-12 y D-23 cerradas. Cero deuda crítica y solo dos altas abiertas (D-09 y D-26).
- ✅ `dotnet build` sin warnings; 95/95 pruebas en verde.
- ⏳ Pendiente (Fase 5): catálogo de ejercicios como entidad de primera clase, que además habilita los récords personales.
- ⏳ Deuda nueva de esta fase: **D-25** (rachas en la zona del servidor) y **D-26** (la API no cubre el tracker).
