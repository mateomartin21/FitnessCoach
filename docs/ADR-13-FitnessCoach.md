# ADR-13: Catálogo de ejercicios como dato, y contenido desacoplado de las estrategias

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 24/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-06 estableció el patrón Strategy para generar rutinas y el ADR-12 modeló el tracker. Este ADR corrige un problema que el ADR-06 dejó latente: las estrategias no solo decidían *cómo* estructurar una rutina, también *contenían* los ejercicios. Este ADR separa el algoritmo del contenido.

---

## Contexto

Después de cuatro fases, agregar un ejercicio nuevo al producto significaba **editar una clase de dominio y recompilar**. Los ejercicios vivían como literales dentro de `EstrategiaGanarMusculo`, `EstrategiaPerderPeso` y `EstrategiaRecomposicion`:

```csharp
new Ejercicio { Nombre = "Press Inclinado con Mancuernas", Series = 4, Repeticiones = "8-10" }
```

Esto tenía tres consecuencias:

- **Cero variedad.** Todos los usuarios con el mismo objetivo recibían exactamente los mismos ejercicios, siempre. No había rotación posible.
- **Nada que mostrar.** Un ejercicio era un nombre y nada más: sin grupo muscular, sin equipo, sin instrucciones, sin demostración visual. Para un principiante — el usuario objetivo declarado en `05-VISION-PRODUCTO.md` — "Sentadilla Búlgara" no dice nada.
- **Sin identidad.** Al no existir el ejercicio como entidad, no se podían registrar récords personales por ejercicio. Ése fue el motivo real por el que la Fase 4 tuvo que posponer ese entregable.

Además, la clase `Ejercicio` mezclaba dos conceptos distintos: **qué es el ejercicio** (nombre) y **cuánto hacer de él** (`Series`, `Repeticiones`). Esa mezcla es la que impedía tratarlo como catálogo: "4 series" no es una propiedad de la sentadilla, es una propiedad de *esta rutina* pidiendo sentadillas.

---

## Decisión

### 1. Dos conceptos, dos tipos

| Tipo | Qué representa | Dónde vive |
|---|---|---|
| `Ejercicio` | El catálogo: slug, nombre, grupo muscular, parte del cuerpo, equipo, músculos secundarios, instrucciones, GIF, video | Tabla `Ejercicios` |
| `EjercicioPrescrito` | Lo que un día de rutina pide: el ejercicio + series + repeticiones + notas | Dentro de la rutina, en memoria |

La separación es lo que habilita todo lo demás: el catálogo se puebla como dato, y la misma sentadilla se prescribe 4×5-8 para fuerza o 3×15 para resistencia sin duplicar nada.

`EjercicioPrescrito` expone `Nombre` como atajo de lectura. Es un detalle menor con un efecto grande: las pruebas del Decorator y las vistas siguieron funcionando **sin cambiar sus asserts**, pese a que el modelo por debajo cambió entero.

### 2. Las estrategias declaran necesidades, no contenido

Cada estrategia pasa de contener ejercicios a describir una plantilla:

```csharp
new("pectorals", 3, 4, "8-10"),   // 3 ejercicios de pectorales, 4 series de 8-10
new("triceps",   2, 4, "10-12")
```

`EstrategiaRutinaBase` compone la rutina resolviendo esas plantillas contra el catálogo. El Strategy sigue siendo Strategy — cada objetivo estructura la semana distinto — pero ahora decide **estructura**, no **contenido**.

Con esto se cumple la Definition of Done de la fase: **agregar un ejercicio no requiere tocar ninguna clase de Strategy**, es una línea en un JSON.

### 3. La selección es determinista por semilla

Las estrategias reciben una `semillaRotacion` (se le pasa el `Id` del perfil) y ordenan los candidatos con un hash estable de `(slug, semilla)`.

- **Dos usuarios con el mismo objetivo reciben ejercicios distintos** — el objetivo textual de la fase.
- **Cada usuario ve siempre su misma rutina.** Si la selección fuera aleatoria en cada carga, la rutina cambiaría al refrescar la página y sería imposible seguir un plan.

No se usa `string.GetHashCode()`: .NET lo aleatoriza por proceso, así que la rutina de cada usuario habría cambiado en cada reinicio del servidor. El hash propio evita ese bug, que además solo se habría manifestado en producción.

### 4. El catálogo se siembra desde un archivo de datos, no desde `HasData`

Se importaron **1.323 ejercicios en español** de [ExerciseGymGifsDB](https://github.com/JahelCuadrado/ExerciseGymGifsDB), servido por jsDelivr con URL pineada a `@v1.1.0`. Todos traen GIF e instrucciones; se verificó que no hubiera slugs duplicados ni entradas sin material antes de importar.

Se eligió `catalogo-ejercicios.json` + un sembrador antes que `HasData` en la migración: con 1.323 registros, `HasData` habría generado decenas de miles de líneas de C# ilegibles en cualquier diff, y cada corrección exigiría una migración nueva. El costo asumido: la semilla no está versionada por EF, así que reponerla exige vaciar la tabla a mano.

Las listas de texto (instrucciones, músculos secundarios) se guardan como JSON en una columna. Son datos de referencia que nadie consulta por elemento ni edita sueltos; una tabla aparte solo agregaría *joins*. Eso exigió un `ValueComparer` explícito: sin él EF compara las listas por referencia y no detecta cambios dentro de la colección.

### 5. Los medios degradan en cadena, nunca en imagen rota

`FabricaMediosEjercicio` produce una cadena ordenada por ejercicio:

```
GIF del CDN → video embebido → enlace de búsqueda en YouTube → placeholder local
```

El servidor **no puede saber** si un GIF remoto va a cargar en el navegador del usuario, así que no elige: entrega la cadena entera y la vista baja de eslabón con `onerror`. Como el último eslabón es un asset propio del proyecto, nunca se llega al ícono de imagen rota — hay una prueba que lo verifica para todas las combinaciones posibles.

El eslabón de YouTube usa una **URL de búsqueda**, no un id de video. Un id inventado da "video no disponible", que es peor que no ofrecer nada; una búsqueda no puede quedar rota. Queda preparado el eslabón de video embebido (`youtube-nocookie.com`, que no planta cookies de seguimiento antes del play) para cuando se carguen ids reales.

Agregar una fuente nueva es sumar un eslabón a la fábrica: ni las vistas ni las estrategias se enteran. Es el mismo patrón que la Fase 6 necesitará para los proveedores de IA, ensayado acá primero.

### 6. Los récords se referencian por slug

`RecordPersonal` guarda `EjercicioSlug`, no el `Id` del catálogo. Los Id los asigna SQL Server al sembrar, así que difieren entre entornos y cambiarían si el catálogo se resembrara. El slug es estable. Se guarda además una copia del nombre para poder mostrar el récord sin consultar el catálogo.

Un índice único sobre `(UsuarioPerfilId, EjercicioSlug)` garantiza en la base que haya **una sola marca vigente por ejercicio**: no se acumulan filas históricas, se actualiza la vigente. Una marca solo se guarda si supera la anterior, comparando por peso y, a igual peso, por repeticiones.

---

## Alternativas Consideradas

### Alternativa 1: Ampliar `Ejercicio` conservando `Series` y `Repeticiones`
Mucho menos trabajo: agregar campos y listo. **Se descarta:** cada fila del catálogo arrastraría series y repeticiones que no significan nada fuera de una rutina concreta, y los récords quedarían atados a ese modelo confuso. El problema volvería en la primera rutina que quisiera prescribir el mismo ejercicio distinto.

### Alternativa 2: Pasar el catálogo como parámetro de `GenerarRutina()`
Cambiar la firma de `IEstrategiaRutina`. **Se descarta:** habría obligado a modificar los tres decoradores y la estrategia falsa de las pruebas. Inyectarlo por constructor deja la interfaz intacta y el Decorator no se entera de que el mundo cambió.

### Alternativa 3: Selección aleatoria de ejercicios
Máxima variedad. **Se descarta:** la rutina cambiaría en cada carga de página, lo que hace imposible seguir un plan y borra la sensación de tener *tu* rutina. El determinismo por semilla da variedad entre usuarios sin sacrificar estabilidad.

### Alternativa 4: Descargar los GIFs al repositorio
Independencia total de la red, como se hizo con Chart.js. **Se descarta:** aun sembrando solo los ejercicios usados serían varios MB de binarios en git, y habría que rebajarlos a mano al agregar ejercicios. Con la cadena de medios, el CDN caído degrada a placeholder sin romper la pantalla, que se consideró suficiente.

### Alternativa 5: ExerciseDB / MuscleWiki para video embebido
Se evaluaron cuatro fuentes buscando GIFs y video. **Se descartan:** `exercisedb.dev` tiene el certificado SSL vencido; MuscleWiki no da API key en su plan gratuito; wger es libre y con licencia clara pero solo tiene 78 videos en `.MOV`/HEVC de ~34 MB, que los navegadores no reproducen de forma confiable. No existe hoy una API gratuita, sin key y con video embebible de ejercicios.

---

## Consecuencias

### Lo que gana el sistema
- **Agregar un ejercicio es editar un JSON.** Ninguna clase de Strategy se toca — la Definition of Done de la fase.
- **1.323 ejercicios con GIF, instrucciones en español, grupo muscular y equipo**, contra los ~30 nombres sueltos anteriores.
- **Dos usuarios con el mismo objetivo ven rutinas distintas**, cada una estable para su dueño.
- **Los récords personales existen** (entregable heredado de la Fase 4), habilitados por la identidad del catálogo.
- **La pantalla de rutinas enseña a entrenar**: miniatura, modal con los pasos, y enlace a video. Antes era una lista de nombres.
- 121 pruebas en verde (26 nuevas), incluidas las de rotación, aislamiento entre cuentas y degradación de medios.

### Lo que se asume o queda pendiente
- **Los GIFs dependen de un CDN de terceros y su licencia no es clara.** El repositorio de origen declara que "los GIFs pertenecen a sus respectivos autores" y que solo provee una capa de organización. Riesgo bajo en contexto académico, pero real si el proyecto se publicara. Registrado como **D-29**.
- **No hay ids de video cargados**, así que el eslabón de embed está escrito pero inactivo: hoy la cadena cae al enlace de búsqueda. Requiere una API key de YouTube Data v3 para resolverlos una vez.
- **Las instrucciones importadas son de plantilla.** Varias son genéricas ("Realiza el movimiento de forma controlada") y no específicas del ejercicio. Es la calidad de la fuente disponible.
- **La alimentación quedó con el mismo problema que este ADR resolvió para ejercicios**: planes hardcodeados dentro de las estrategias (**D-28**) y, peor, un rango calórico fijo que ignora el cálculo personalizado del usuario (**D-27**). Detectado al cerrar esta fase.
- El catálogo se carga completo en cada consulta por grupo muscular. A esta escala es irrelevante; si creciera, corresponde caché (previsto en la Fase 10).

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ Catálogo de 1.323 ejercicios persistido, con puerto, adaptador SQL de solo lectura y doble adaptador en pruebas.
- ✅ Las tres estrategias componen desde el catálogo con rotación estable por usuario.
- ✅ Récords personales por ejercicio, con una marca vigente garantizada por índice único.
- ✅ `dotnet build` sin warnings; 121/121 pruebas en verde.
- ⏳ Pendiente (Fase 6): resiliencia de IA — que además reutiliza el patrón de cadena de proveedores ensayado acá.
- ⏳ Deuda nueva de esta fase: **D-27**, **D-28** (alimentación) y **D-29** (licencia de los GIFs).
