# ADR-20: Cierre del producto — rendimiento medido, calendario del usuario, API completa y accesibilidad

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 30/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** este ADR cierra deuda abierta por otros. El ADR-11 dejó el rate limiter en memoria y sin cabeceras de proxy (D-24); el ADR-12 dejó las fechas contadas en la hora del servidor (D-25) y la API sin cubrir el tracker (D-26); el ADR-19 dejó el halo de los sprites (D-30) y los PNG de branding sin uso (D-31). Es la Fase 10 del roadmap, la última.

---

## Contexto

La Fase 10 no agrega funcionalidad: cierra. Tres deudas venían anotadas desde fases anteriores y el roadmap pedía además
rendimiento (N+1, índices, caché), estáticos, accesibilidad y el cierre documental.

La regla que guió toda la fase fue **medir antes de tocar**. Dos hallazgos importantes no estaban en ninguna lista de
deuda y aparecieron solo porque se midió el comportamiento real en vez de leer el código:

1. Leer **un** perfil costaba una consulta con **cuatro `LEFT JOIN` entre colecciones sin relación entre sí** —un
   producto cartesiano— y esa lectura se repetía hasta seis veces por pantalla.
2. La fecha del diario de comidas **no es un instante**, es la etiqueta del día que eligió el usuario. El código la
   trataba como instante y la corría un día, así que la comida del **lunes** nunca contaba en la misión semanal.

También apareció que dos deudas estaban **mal registradas**, cosa que solo se ve al ir a arreglarlas (ver más abajo).

---

## Decisión

### 1. Rate limiter listo para producción, con el límite en memoria aceptado a propósito (D-24)

Se configuró `UseForwardedHeaders` para `X-Forwarded-For` y `X-Forwarded-Proto`, confiando **solo** en los proxies
declarados por configuración (`ForwardedHeaders:KnownProxies` / `KnownNetworks`). La lista por defecto se limpia: aceptar
esas cabeceras de cualquier cliente permitiría falsificar la IP y evadir el límite. Sin esto, detrás de un balanceador
todo el tráfico se contaría bajo la IP del proxy y el límite sería inútil.

**El estado del limitador sigue en la memoria del proceso, y eso es una decisión, no un olvido.** Con una sola instancia
—el despliegue previsto— alcanza. Un almacén compartido (Redis) resolvería el caso multi-instancia, pero agrega
infraestructura que no está en el stack ni en el presupuesto del proyecto. Queda escrito en el código y en la deuda.

### 2. El calendario es el del usuario, no el del servidor (D-25)

`UsuarioPerfil.ZonaHoraria` guarda un id **IANA** (`America/Mexico_City`), y `ZonaHorariaUsuario` (Application) es el
**único** lugar que decide qué día es para el usuario: resuelve la zona con caída a la de por defecto y luego a UTC sin
lanzar nunca, y convierte las marcas guardadas.

La zona se elige en el perfil, con **autodetección del navegador** (`Intl.DateTimeFormat().resolvedOptions().timeZone`):
si el perfil no tiene zona se preselecciona la detectada, y si no está entre las comunes se agrega como opción. Si el
usuario eligió otra a mano, se respeta. El POST solo guarda ids que el sistema reconozca.

Se cablearon los seis lugares que leían el reloj del servidor: rachas, snapshot de gamificación (incluido el lunes con
que reinician las misiones), el bloque semanal y el "diario de hoy" del contexto de Koda, el "hoy" del diario, y las
fechas que se muestran en Progreso.

**Excepción explícita: el diario no se convierte de zona.** `ServicioDiario` guarda la fecha como la medianoche del día
elegido, o sea es una *etiqueta de día*, no un instante. Convertirla la corría hacia atrás.

### 3. La regla del entrenamiento válido baja al servicio, y la API cubre el tracker (D-26)

La API pasó a cubrir el tracker completo: `GET {id}`, `PUT {id}` y `DELETE {id}` de registros de peso, y un controlador
nuevo de entrenamientos con historial, rachas, opciones de rutina, alta y borrado. Todo reusando `IServicioProgreso` e
`IServicioEntrenamientos`, que ya tenían el aislamiento por cuenta.

Antes de eso hubo que mover una regla. La validación de "solo se registra un día real de tu rutina" —que el ADR-18 puso
tras un review del usuario, porque con texto libre cualquiera ganaba XP y logros sin entrenar— vivía en
`ProgresoController`. Una API nueva la habría salteado y reabierto el agujero. Ahora `ServicioEntrenamientos.Registrar`
la hace cumplir (lanza `ArgumentException`) y expone `OpcionesDeRutina`, que consumen la pantalla y la API por igual.

**Al reusar el servicio se corrigió otra inconsistencia:** el POST de la API armaba el registro a mano y **no
sincronizaba el peso del perfil**, cosa que la pantalla sí hacía. Dar de alta un peso por API dejaba el cálculo calórico
corriendo con el peso viejo.

### 4. Rendimiento: `AsSplitQuery`, caché de catálogos y una lectura de perfil por petición

Tres cambios, todos con medición antes y después:

- **`AsSplitQuery` al leer el perfil.** Se volcó el SQL real: el perfil trae cuatro colecciones owned y EF las incluye
  siempre, en una sola sentencia con cuatro `LEFT JOIN` entre tablas sin relación entre sí. Con 100 comidas, 50
  entrenamientos, 50 pesos y 10 récords son 2.5 millones de filas para leer un perfil.
- **Caché de los dos catálogos, por Decorator.** El puerto no apunta al adaptador SQL sino a un decorador que lo
  envuelve; la lógica de los índices es pura y vive en `Domain/Catalogos` (`IndiceEjercicios`, `IndiceAlimentos`), donde
  las pruebas la alcanzan. Los índices comparan **sin distinguir mayúsculas**, como la colación de SQL Server: con un
  diccionario ordinal, `"Pecho"` y `"pecho"` dejarían de coincidir y las rutinas se armarían con menos ejercicios sin
  que falle nada.
- **`ServicioPerfilUsuario` recuerda el perfil que ya leyó.** Es scoped, así que la memoria dura una petición. No cambia
  el comportamiento: EF ya devolvía la misma instancia rastreada en todas esas lecturas.

Medido con el log de EF, mismo usuario y mismos datos:

| Pantalla | Antes | Después |
|---|---|---|
| `/Progreso` | 20 consultas | 6 (5 con la caché caliente) |
| `/Rutinas` | 15 | 5 |
| `/Diario` | 30 | 6 (5 con la caché caliente) |

**Índices: no se tocó ninguno.** Ya estaban los que piden las consultas reales: `Slug` único en ambos catálogos,
`GrupoMuscular`, `Categoria`, `GrupoIntercambio`, `Equipo`, las FK de las cuatro colecciones owned, y un único filtrado
en `IdentityUserId`.

### 5. Estáticos: el halo era de todos los sprites, y dos deudas estaban mal registradas

**D-30 estaba subregistrada.** Decía "un par de sprites"; eran los 26. Mirando los píxeles crudos, los bordes eran
**blanco puro (253-254) con alfa 1-6**: el resto del fondo blanco que se quitó. Se midió la distribución antes de elegir
el umbral —con alfa ≤32 el 90% de esos píxeles es residuo, y arriba de 64 ya es antialias legítimo del pelaje claro del
lobo— y solo se anulan los casi-blancos de alfa ≤32. Son 183.386 píxeles.

Los sprites tienen 15.000 a 42.000 colores (son imágenes con antialias, no pixel art puro) y se muestran a 88-200 px.
Cuantizados a paleta de 256: **1722 KB → 293 KB**. Se comparó visualmente el peor caso antes de aceptarlo, incluida una
pose con una tablet translúcida, que es lo que un defringe mal calibrado habría destruido.

**D-31 estaba mal en los dos PNG que nombraba.** Decía que ninguno se usaba, pero `branding/logo.png` era el último
eslabón de `FabricaMediosEjercicio`: el placeholder que aparece cuando falla el GIF de un ejercicio. Y era el **logo
naranja anterior a la identidad azul de la Fase 9**, así que justo cuando una imagen fallaba aparecía el branding que esa
fase había barrido. Ahora el placeholder es un sprite de Koda y la carpeta se borró.

De paso, el **favicon era el genérico de la plantilla de ASP.NET**. Se generó desde la cara de Koda en 16/32/48 px.

### 6. Accesibilidad: teclado, lectores de pantalla y contraste AA

- 110 iconos de Font Awesome marcados `aria-hidden="true"`: son decorativos y el texto al lado ya dice lo mismo.
- Etiquetas asociadas por `for`/`id` en los cuatro campos donde el `<label>` existía pero no estaba ligado.
- Nombre accesible en los botones que solo tenían icono (menú, enviar del chat, mostrar contraseña). El de la contraseña
  además tenía `tabindex="-1"`: era un control solo para mouse. Ahora es alcanzable y refleja su estado con
  `aria-pressed`.
- Enlace **"Saltar al contenido"** como primer tabulador, y foco visible con `:focus-visible` (los campos ya lo tenían;
  enlaces y botones no).
- `role="status" aria-live="polite"` en el análisis de Koda y `role="log"` en el chat: llegan por `fetch` y sin eso un
  lector de pantalla no los anuncia.
- `scope="col"` en los 19 encabezados de tabla.
- **Contraste:** se calcularon los ratios de todos los tokens. El azul primario da **4.19:1** sobre el fondo y AA pide
  4.5 para texto normal. Se agregó `--fc-primary-texto: #5b8cff` (5.95:1 sobre el fondo, 5.30:1 sobre las tarjetas) para
  los 46 usos donde el azul pinta **texto**; `--fc-primary` se queda para rellenos y bordes, donde el mínimo es 3:1.

### 7. Los comentarios se recortan a lo que no se deduce del código

Decisión del usuario al revisar el diff: sobraban comentarios y varios "sonaban a IA" —narrativos, con escenarios
hipotéticos, repitiendo lo que el nombre ya dice. La regla que queda escrita en `03-ESTANDARES.md`: **una o dos líneas
para el por qué que no se deduce del código, con la referencia `D-xx` cuando aplique, y nada más.** Se exceptúan los
`<summary>`/`<response>` de los controladores de API, porque alimentan el OpenAPI.

Se aplicó a los 33 archivos de la fase: 206 líneas de comentario fuera, 97 más concisas en su lugar.

### 8. Despliegue: SQL Server Express, y PostgreSQL como deuda

El usuario planteó que pagar SQL Server no era opción. **Se despliega con SQL Server Express**, que no tiene costo de
licencia y limita a 10 GB por base y 1 GB de RAM de buffer: para esta app (catálogos de ~1400 filas más los datos de los
usuarios) sobra, y no requiere ni un cambio de código.

PostgreSQL queda registrado como deuda, ya anticipado por el ADR-06 y el ADR-07. El costo de migrar no es solo cambiar el
proveedor: **PostgreSQL distingue mayúsculas y SQL Server no**, así que las búsquedas por slug cambiarían de
comportamiento (la caché de esta fase ya lo normaliza, pero el sembrador no), y las migraciones usan tipos y sintaxis de
SQL Server (`nvarchar`, `datetime2`, `HasFilter("[IdentityUserId] IS NOT NULL")`). Habría que regenerarlas con Npgsql, no
editarlas a mano.

---

## Alternativas Consideradas

### Alternativa 1: Redis para el rate limiter
Resolvería el caso multi-instancia de verdad. **Se descarta:** agrega un servicio a operar y pagar para un despliegue de
una sola instancia. El riesgo real (el límite multiplicado por la cantidad de nodos) no existe mientras haya un nodo.

### Alternativa 2: `AsNoTracking` en la lectura del perfil
Era el reflejo obvio para "rendimiento de EF". **Se descarta:** el perfil se modifica y se guarda a través del change
tracker; sin seguimiento habría que reimplementar `Guardar`. El problema no era el seguimiento sino el cartesiano y la
repetición de lecturas.

### Alternativa 3: Cachear el perfil del usuario, no solo los catálogos
Habría bajado más las consultas. **Se descarta:** el perfil **sí** cambia en caliente, y una caché con vigencia mostraría
datos viejos justo después de registrar algo. La memoria por petición da el ahorro sin ese riesgo.

### Alternativa 4: Poner los índices de catálogo en `Application` para poder probarlos
Fue el primer intento. **Se descarta:** los usa un adaptador de `Infrastructure`, que solo referencia `Domain`. Antes que
agregar una dependencia nueva entre capas, los índices quedaron en `Domain/Catalogos`, que ambos ya referencian.

### Alternativa 5: Bajar el azul primario para que pase AA
Un solo token cambiado y listo. **Se descarta:** `--fc-primary` es la identidad de la Fase 9 y pinta botones, bordes y
glows, donde 4.19:1 ya cumple el mínimo de 3:1 de componentes. Aclararlo todo habría cambiado el look para arreglar solo
el texto.

### Alternativa 6: Usar las rutas con huella de `MapStaticAssets` para cachear estáticos un año
El manifiesto ya las genera con `max-age=31536000, immutable`. **Se descarta por ahora:** el helper `@Assets` que las
resuelve **no existe en vistas MVC** (es de Blazor), y no vale inventar un esquema propio de fingerprinting. Queda como
deuda; hoy hay ETag, gzip y una hora de caché en imágenes.

---

## Consecuencias

### Lo que gana el sistema
- **Cero deuda crítica y cero deuda alta abiertas.** D-24, D-25, D-26, D-30 y D-31 cerradas.
- **Las rachas, las misiones y el "hoy" del diario son los del usuario**, en cualquier zona horaria y con el servidor en
  UTC. Es la condición para desplegar fuera de la máquina de desarrollo.
- **Tres a seis veces menos consultas** en las pantallas más pesadas, y sin productos cartesianos.
- **La API sirve para construir un cliente de verdad** (una app móvil, el caso que el roadmap dejó fuera de alcance) y
  cumple las mismas reglas que la pantalla, incluida la que evita ganar XP sin entrenar.
- **`wwwroot` sin `lib/` pasó de 2149 KB a 328 KB**, y los sprites ya no arrastran el halo del fondo original.
- **Navegable con teclado y anunciable por un lector de pantalla**, con el texto en contraste AA.
- **Dos bugs silenciosos corregidos**: la comida del lunes que no contaba, y el peso del perfil que la API no
  sincronizaba.
- 348/348 pruebas en verde (+25 en la fase) y **los 7 puntos de la prueba de fuego corridos contra la app real**.

### Lo que se asume o queda pendiente
- **El rate limiter sigue en memoria** (asumido arriba). Con más de una instancia, el límite efectivo se multiplica por
  la cantidad de nodos.
- **Los estáticos no usan las rutas inmutables** de `MapStaticAssets`: sirven con ETag, gzip y `max-age=3600` en
  imágenes. Registrado como **D-33**.
- **Font Awesome se carga de `cdnjs.cloudflare.com`**, contra la regla del proyecto de copiar las libs de front a
  `wwwroot/lib/`. Si el CDN no responde, desaparecen todos los iconos. Autohospedarlo suma ~1-2 MB de webfonts; es una
  decisión de producto, no técnica. Registrado como **D-34**.
- **`PersonalidadLoboCoach` conserva el nombre viejo** del coach, que desde la Fase 9 se llama Koda. Es cosmético y
  renombrarlo toca pruebas. Registrado como **D-32**.
- **Los adaptadores de caché no tienen pruebas unitarias** (ADR-08: `FitnessCoach.Tests` no referencia
  `Infrastructure`). La lógica que importa —los índices— sí está cubierta; el envoltorio de `IMemoryCache` se verificó a
  mano contra un repositorio que cuenta consultas: 15 llamadas, una sola consulta.
- **PostgreSQL sigue pendiente** como opción de base (D-35), con el detalle de sensibilidad a mayúsculas anotado arriba.
- **El despliegue a EC2 no forma parte de esta fase**: el roadmap lo lista como idea fuera de alcance. Lo que la fase
  entrega es una app que *puede* desplegarse (cabeceras de proxy, zona horaria del usuario, persistencia verificada tras
  reiniciar).

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ D-24 cerrada: `UseForwardedHeaders` con proxies de confianza; límite en memoria asumido y documentado.
- ✅ D-25 cerrada: zona horaria en el perfil, con autodetección, y los días contados en el calendario del usuario.
- ✅ D-26 cerrada: la API cubre el tracker; la regla del entrenamiento válido vive en el servicio.
- ✅ D-30 y D-31 cerradas: sprites sin halo y 83% más livianos; placeholder y favicon con la identidad de Koda.
- ✅ Rendimiento medido: `AsSplitQuery`, caché de catálogos y una lectura de perfil por petición.
- ✅ Accesibilidad: teclado, lectores de pantalla y contraste AA.
- ✅ Comentarios recortados a lo que no se deduce del código; regla escrita en los estándares.
- ✅ Diagramas C4 actualizados (estaban en el estado de la Fase 0) y README reescrito.
- ✅ 348/348 pruebas; los 7 puntos de la prueba de fuego corridos y en verde.
- ⏳ Deuda nueva, toda de prioridad baja: D-32 (nombre de la clase de personalidad), D-33 (caché de estáticos), D-34
  (Font Awesome por CDN), D-35 (PostgreSQL como opción de base).
- 🏁 **Con esta fase se cierra el roadmap.** Lo que siga son las ideas fuera de alcance de `06-ROADMAP.md`.
