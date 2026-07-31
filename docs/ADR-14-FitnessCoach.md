# ADR-14: Nutrición personalizada — plan de comidas calculado desde el catálogo y el perfil

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 24/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-13 separó el algoritmo del contenido para los ejercicios (catálogo como dato, estrategias que declaran estructura). Este ADR aplica el mismo patrón a la alimentación, que había quedado con exactamente el problema que el ADR-13 resolvió — y lo lleva un paso más allá: el plan no solo se compone desde un catálogo, además se **calcula** a partir del perfil de cada usuario. Cierra las deudas **D-27** y **D-28** que el propio ADR-13 dejó anotadas.

---

## Contexto

Al cerrar la Fase 5 quedaron registradas dos deudas sobre la alimentación:

- **D-28:** los planes vivían escritos a mano dentro de `AlimentacionPerderPeso`, `AlimentacionGanarMusculo` y `AlimentacionRecomposicion`. Los alimentos eran `List<string>` de texto plano (`"180g pechuga de pollo a la plancha"`) y los macros de cada comida estaban **sumados a mano**. No había entidad `Alimento`, ni catálogo, ni forma de sustituir nada.
- **D-27, la más grave:** cada estrategia declaraba un rango calórico fijo — `CaloriasObjetivo = "1800-2000 kcal/día"` — idéntico para todos. `CalculadorCaloricoService` ya calculaba el requerimiento real de cada persona y la app lo mostraba en la pantalla de Perfil, pero el plan de comidas lo ignoraba. **La app se contradecía a sí misma en pantallas contiguas.**

El objetivo de la fase no fue solo pagar esas deudas, sino tomarse en serio el apartado: que el plan se parezca a lo que entrega un nutricionista, no a un folleto genérico.

---

## Decisión

### 1. Los macros se calculan en el orden que usa un profesional

`CalculadorMacros` reparte las calorías diarias en gramos de proteína, grasa y carbohidrato, y no lo hace por porcentajes arbitrarios:

1. **La proteína se fija por peso corporal** (1,6–2,2 g/kg según el objetivo). Es el único macro con requerimiento absoluto: 70 kg × 2,2 = 154 g, se coman 1.800 o 2.600 kcal.
2. **La grasa se fija como porcentaje de las calorías**, con un piso del 15% por debajo del cual se compromete la función hormonal.
3. **Los carbohidratos absorben el resto**: son la energía flexible.

Cada objetivo define su factor proteico y su porcentaje de grasa como miembros abstractos de `ObjetivoFitness` — pérdida de grasa 2,2 g/kg (déficit: máxima protección del músculo), volumen 1,8 (con superávit sobra energía), recomposición 2,0. **El Strategy ahora decide también el perfil nutricional**, coherente con el patrón que ya gobernaba las rutinas.

Hay una guarda para el caso que rompe las fórmulas: una persona muy pesada con pocas calorías, donde proteína y grasa solas se comen todo el presupuesto. Sin ella, los carbohidratos salían **negativos** — un plan imposible. Se recorta la proteína hasta un piso de 1,2 g/kg antes que devolver un número absurdo.

### 2. El catálogo de alimentos es un dato, con macros verificables

Se sembraron **67 alimentos** con sus macros por 100 g. La unidad de 100 g es la referencia de toda la nutrición seria: permite comparar y escalar a cualquier porción con una regla de tres.

- **Los macros vienen de USDA FoodData Central** (volcado SR Legacy, dominio público). Se descartó la API de USDA: la `DEMO_KEY` corta a las 30 peticiones/hora. El volcado no pide clave y es reproducible.
- **La lista de qué entra es curada a mano.** USDA tiene cientos de miles de entradas, en su mayoría productos de marca que empeorarían el plan. 67 alimentos reales con grupos de intercambio valen más que 400.000 códigos de barras.
- **Las calorías no se guardan: se calculan** desde los macros con los factores de Atwater (4/4/9). Guardadas aparte podrían quedar en desacuerdo con los gramos y el plan mostraría un total que no se corresponde con sus alimentos. Calculadas, es imposible.

Mismo criterio que el ADR-13: `catalogo-alimentos.json` + sembrador antes que `HasData`. Agregar o corregir un alimento es editar un JSON.

### 3. Las estrategias declaran la estructura del día, no las comidas

Igual que las rutinas declaran plantillas de ejercicios, las estrategias de alimentación declaran plantillas de comidas: cuántas, a qué hora, qué papel cumple cada alimento (`RolAlimento`) y qué parte del total aporta la comida. `EstrategiaAlimentacionBase` compone el plan resolviendo esas plantillas contra el catálogo y **escalando las porciones a los macros calculados**.

El reparto dentro de cada comida sigue el mismo orden que `CalculadorMacros`: primero se sirve la proteína (requerimiento absoluto), el carbohidrato con lo que quede, la grasa al final. Verduras y frutas van en porción habitual — su papel es el volumen y los micronutrientes, no cuadrar una cuenta.

Con esto muere el `"1800-2000 kcal/día"`: dos usuarios con el mismo objetivo y distinto peso reciben planes distintos, y cada uno ve siempre el suyo (selección determinista por `Id`, como en el ADR-13).

### 4. Los alimentos declaran en qué comida caen bien

Cuadrar los macros no alcanza. Al inspeccionar planes reales, el desayuno proponía *"115 g de tempeh con pasta integral y mango"*: correcto en números, y nadie lo desayuna. **Un plan que no se sigue no sirve de nada.**

Cada alimento declara sus `MomentosAptos` (`desayuno` / `principal` / `snack`) — criterio culinario, no nutricional — y el motor filtra el catálogo por el momento de la comida. El mismo perfil pasó a desayunar mango, huevo y tortilla de maíz.

### 5. Sustituciones por equivalencia de macros

Cada porción del plan trae alternativas del mismo grupo de intercambio, escaladas para aportar lo mismo del macro que define al alimento: *"en vez de 150 g de pollo, 190 g de merluza"*. Es la tabla de intercambios del nutricionista, calculada sola por `CalculadorEquivalencias`.

- **Qué se iguala lo decide el propio alimento.** El "macro principal" (`Alimento.MacroPrincipal`) es una sola fuente de verdad: define a la vez qué rol cumple el alimento en el plan y con qué se lo puede cambiar. Un cereal iguala carbohidratos; una proteína, proteína; un lácteo se resuelve por lo que realmente aporta (yogur→proteína, ricotta→grasa).
- **Se descarta la equivalencia de papel.** Sustituir 150 g de pollo por 400 g de brócoli iguala la proteína en el número y no es un plato que nadie coma. Si la porción cae fuera del mínimo/máximo del alimento, no se ofrece.
- **Los candidatos respetan el momento**, para no proponer avena de cena en lugar del arroz.

### 6. El plan es orientativo y lo dice

Un plan calculado desde fórmulas no sabe de patologías, medicación, embarazo ni alergias. La vista muestra un **descargo visible** (`PlanAlimentacion.Descargo`) de que es orientativo y no reemplaza a un profesional. Es parte de hacer esto en serio, no letra chica.

### 7. Las imágenes vienen de Wikimedia, no de Open Food Facts

Se había elegido USDA + Open Food Facts. Al probarlo, OFF resultó ser una base de **productos envasados**: buscar "banana" devuelve *Fromage Blanc*, agua mineral y yogures de marca. Sirve para un producto con código de barras, no para el alimento genérico "brócoli". Se cambió a **Wikimedia Commons** (vía Wikipedia en español), que devuelve la foto correcta. Cada imagen guarda su autor y licencia porque son CC BY-SA: atribuir es condición de uso, y la vista la muestra.

---

## Alternativas Consideradas

### Alternativa 1: Importar el catálogo completo de USDA
Máxima cobertura. **Se descarta:** cientos de miles de entradas, en su mayoría productos de marca (`"Chobani Greek Yogurt Strawberry"`) que ensucian la elección sin mejorar el plan. Una lista curada de 67 alimentos base cubre todas las categorías y grupos de intercambio.

### Alternativa 2: Usar la API de USDA en vez del volcado
Datos siempre frescos. **Se descarta:** la `DEMO_KEY` corta a las 30 peticiones/hora (se murió importando el alimento 12) y pedir una key propia ata la reproducibilidad de la siembra a un servicio externo. El volcado SR Legacy es de dominio público, sin clave, y la semilla queda versionada como archivo.

### Alternativa 3: Open Food Facts para las imágenes
Era la fuente elegida. **Se descarta tras probarla:** es una base de productos envasados; para alimentos genéricos devuelve marcas irrelevantes. Wikimedia da la foto del alimento real.

### Alternativa 4: Guardar las calorías de cada alimento como columna
Un cálculo menos por porción. **Se descarta:** una caloría guardada puede quedar en desacuerdo con sus gramos de macro, y entonces el plan miente sobre su propio total. Derivadas con Atwater, esa contradicción es imposible.

### Alternativa 5: Un solo alimento proteico por comida
Más simple. **Se descarta:** con topes de porción realistas, alguien de 120 kg en déficit necesita ~79 g de proteína en el almuerzo, que serían 350 g de pollo cuando el máximo razonable es 250. El plan se quedaba corto justo con quien más lo necesita. Se agrega una segunda fuente de proteína al plato — lo mismo que hace un nutricionista — con tope de dos refuerzos para que no se vuelva una lista.

---

## Consecuencias

### Lo que gana el sistema
- **El plan de comidas es de cada usuario.** Escala a las calorías y los macros calculados desde su peso y objetivo. Cierra **D-27**, la deuda alta que hacía que la app se contradijera a sí misma.
- **Agregar o cambiar un alimento es editar un JSON.** Ninguna clase de Strategy se toca. Cierra **D-28**.
- **67 alimentos con macros de USDA, imágenes atribuidas y grupos de intercambio**, contra las `List<string>` de texto plano anteriores.
- **Sustituciones equivalentes** en cada porción: la tabla de intercambios del nutricionista, calculada sola.
- **Planes que se pueden seguir**: los alimentos respetan el momento del día, no solo los macros.
- **Descargo médico visible** en cada plan.
- 227 pruebas en verde (+69 en la fase), incluidas las que validan el JSON de la semilla y las que corren el generador contra el catálogo real con cinco perfiles distintos.

### Lo que se asume o queda pendiente
- **No hay preferencias ni exclusiones todavía.** Un vegetariano o alguien con alergia recibe el mismo catálogo que el resto. Las etiquetas de dieta (`EtiquetasDieta`) ya están sembradas en cada alimento esperando ese filtro; es el trabajo de la **Fase 5.6**.
- **No se registra la adherencia.** El plan se muestra pero no se puede marcar qué se comió ni seguir los macros del día. También Fase 5.6.
- **El plan es de un día tipo**, no un menú semanal con variación entre días. La estructura lo permite (la semilla de rotación podría incluir el día), pero no está hecho.
- **Las porciones se acercan al objetivo, no lo clavan.** Se sirven en cantidades realistas y redondeadas a 5 g, así que el total queda dentro de ±20% del objetivo. Mostrar ese desvío se consideró más honesto que fingir exactitud.

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ `CalculadorMacros` reparte proteína/grasa/carbohidrato desde el peso y el objetivo, con pisos de seguridad.
- ✅ Catálogo de 67 alimentos persistido, con puerto, adaptador SQL de solo lectura y doble adaptador en pruebas.
- ✅ Las tres estrategias componen el plan desde el catálogo y escalan las porciones a los macros del usuario.
- ✅ Sustituciones por equivalencia de macros en cada porción, acotadas por grupo de intercambio y momento del día.
- ✅ Filtro de momento del día y descargo médico visible.
- ✅ `dotnet build` sin warnings; 227/227 pruebas en verde.
- ⏳ Pendiente (Fase 5.6): preferencias y exclusiones (vegetariano, sin gluten, alergias) + registro de adherencia con seguimiento de macros del día.
- ⏳ Pendiente (Fase 6): resiliencia de IA (D-09, D-20).
