# ADR-15: Preferencias alimentarias y diario de adherencia

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 25/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** El ADR-14 hizo que el plan de comidas se calculara desde el perfil y se compusiera desde el catálogo. Este ADR cierra el apartado de nutrición con las dos piezas que faltaban para que el plan sea de verdad de cada persona: que respete lo que **puede y quiere** comer, y que sirva para **seguir** si lo cumple. Reutiliza el catálogo, las etiquetas de dieta y el motor de composición que el ADR-14 dejó listos.

---

## Contexto

Al cerrar la Fase 5.5 el plan ya se adaptaba al peso y al objetivo, pero seguía siendo el mismo para todos en dos aspectos:

- **Ignoraba lo que la persona puede comer.** Un vegetariano recibía pollo; alguien con alergia al maní, maní. Las etiquetas de dieta (`EtiquetasDieta`) ya estaban sembradas en cada alimento, pero nada las usaba.
- **No se podía seguir.** El plan se mostraba y ahí terminaba: no había forma de registrar qué se comió ni de ver cómo iba el día contra el objetivo. Un plan que no se puede seguir es medio plan.

El usuario eligió el alcance más completo para la adherencia: no solo marcar las comidas del plan, sino un **diario libre** donde registrar cualquier alimento del catálogo.

---

## Decisión

### 1. Las preferencias son un objeto de valor con la regla de "esto se puede comer"

`PreferenciasAlimentarias` vive en el perfil (como `OwnsOne`, sin identidad propia) y encapsula dos criterios distintos:

- **Dietas seguidas** — reglas amplias (`vegetariano`, `vegano`, `sin-gluten`, `sin-lactosa`). Un alimento pasa solo si cumple **todas** las que el usuario sigue.
- **Alimentos excluidos** — vetos puntuales por slug, para una alergia o algo que no gusta. Pesan por encima de todo: un alimento vetado no pasa aunque cumpla las dietas.

La lógica vive en el método `Permite(Alimento)`, con su propia batería de pruebas. No es un `if` suelto en la estrategia: es una regla de negocio con nombre y casa.

### 2. El filtro va en un solo lugar, arriba de todo

El motor (`EstrategiaAlimentacionBase`) aplica `Permite` en `Elegir`, **antes** del filtro de momento del día y de los *fallbacks*. Esa posición es deliberada: nada de lo que viene después —ni repetir un alimento cuando falta variedad, ni caer al catálogo entero cuando el momento no da opciones— puede devolver algo vetado. Las sustituciones se filtran igual: no tiene sentido ofrecer como alternativa algo que el usuario excluyó.

Un único punto de filtrado significa que la garantía "un vegetariano nunca ve carne" no depende de acordarse de repetir la comprobación en cada camino.

### 3. El diario guarda un *snapshot*, no una referencia viva

`RegistroComida` copia los macros del alimento al momento de registrarlo, en vez de referenciar el catálogo. *"Lo que comí ayer"* es un hecho del pasado: no debe cambiar si mañana se corrige la ficha del alimento. Es el mismo criterio con que `RecordPersonal` (ADR-13) copia el nombre del ejercicio.

El único camino para crear un registro es `RegistroComida.De(alimento, gramos, fecha)`, que escala los macros a la cantidad comida. Así los macros guardados corresponden siempre a los gramos, sin sumas a mano —la misma disciplina que el ADR-14 impuso en las porciones del plan.

### 4. El resumen del día es cálculo puro

`ResumenDiario` recibe los registros de un día y el objetivo de macros, y expone lo consumido, lo que falta y si la persona se pasó. No toca base de datos ni catálogo: es una función sobre números, testeable sin montar nada. `ServicioDiario` (en Application) es el que orquesta —busca el alimento en el catálogo, calcula el objetivo desde el perfil, persiste— y delega la cuenta en el dominio.

Si el perfil todavía no tiene datos válidos para calcular calorías, el resumen devuelve un objetivo en cero en vez de romper: el diario sirve para anotar aunque no haya una meta contra la cual comparar.

### 5. El diario acepta el plan y el catálogo entero

Dos formas de registrar, según lo que el usuario esté haciendo:

- **Una comida del plan de un toque.** Como el plan es determinista, regenerarlo devuelve las mismas comidas que vio, y se registran todas sus porciones juntas.
- **Cualquier alimento del catálogo**, con su cantidad. Es el "diario libre" que se eligió como alcance: la vida real no siempre sigue el plan.

El POST solo acepta slugs que existen y cantidades razonables (1–2000 g); un formulario manipulado no puede meter basura. Mismo criterio que la pantalla de preferencias, que solo acepta dietas conocidas y slugs del catálogo.

---

## Alternativas Consideradas

### Alternativa 1: Filtrar las preferencias en la vista o el controlador
Ocultar en pantalla lo que el usuario excluyó. **Se descarta:** el plan se *compone* eligiendo alimentos para cuadrar macros; si el filtro fuera cosmético, la estrategia igual gastaría "cupo" de una comida en un alimento vetado y después lo escondería, dejando la comida incompleta. El filtro tiene que estar en la selección, no en la presentación.

### Alternativa 2: El diario referencia el catálogo por slug, sin copiar macros
Menos columnas. **Se descarta:** el diario es un registro histórico. Si el lunes corrijo los macros de un alimento, no quiero que cambien retroactivamente las calorías que el diario dice que comí el domingo. El *snapshot* es lo que hace que el historial sea fiel.

### Alternativa 3: Solo marcar comidas del plan, sin diario libre
Más simple. **Se descarta por decisión de alcance:** la persona no siempre come lo que el plan dice, y un diario que solo acepta el plan mide obediencia, no lo que realmente pasó. El registro libre refleja la realidad, que es lo que un seguimiento serio necesita.

### Alternativa 4: `PreferenciasAlimentarias` como entidad con tabla propia
Consistencia con las otras colecciones. **Se descarta:** las preferencias son un dato por usuario, no una colección de filas con identidad. `OwnsOne` las guarda en la misma fila del perfil, sin una tabla ni un join que no aportan nada.

---

## Consecuencias

### Lo que gana el sistema
- **El plan respeta lo que la persona puede comer.** Un vegetariano con alergia recibe un plan completo que nunca incluye lo excluido, ni como comida ni como sustituto. Verificado contra el catálogo real.
- **El plan se puede seguir.** El diario registra lo comido —del plan o libre— y muestra el día contra el objetivo con lo consumido, lo que falta y si se pasó.
- **El apartado de nutrición queda cerrado**: cálculo de macros (5.5) → catálogo (5.5) → plan personalizado (5.5) → preferencias y adherencia (5.6).
- 259 pruebas en verde (+32 en la fase), incluidas las de un vegetariano con alergia y un vegano contra el catálogo real, y las del servicio de diario y el resumen.

### Lo que se asume o queda pendiente
- **El catálogo no tiene proteínas veganas de desayuno.** Al filtrar por vegano, ninguna proteína del catálogo está marcada como apta para el desayuno (no hay huevo ni lácteos veganos), así que el *fallback* de momento sirve garbanzos o legumbres a la mañana. Es correcto pero culturalmente raro; se resolvería enriqueciendo el catálogo (yogur vegetal, bebida de soja), no cambiando la lógica.
- **El día se cuenta en la zona horaria del servidor** (`DateTime.Today`), la misma limitación que **D-25** ya registra para las rachas. Se resolverá junto con ella cuando el perfil guarde la zona del usuario.
- **No hay objetivos por comida**, solo por día: el diario compara el total diario, no cuánto llevabas al almuerzo.
- **La adherencia no se historiza como métrica.** Se puede ver cualquier día pasado, pero no hay todavía una vista de "cumpliste X de los últimos 7 días" — candidata natural para la Fase 7 (IA que analiza datos reales) o la 8 (gamificación).

---

## Estado actual del proyecto (avances tras este ADR)

- ✅ `PreferenciasAlimentarias` en el perfil, con filtrado en el motor y pantalla para editarlas.
- ✅ El plan y las sustituciones respetan dietas y exclusiones, verificado contra el catálogo real.
- ✅ `RegistroComida` (diario) como colección owned, con snapshot de macros y `ServicioDiario`.
- ✅ `ResumenDiario` puro: consumido vs objetivo, restante, si se pasó.
- ✅ Pantalla de diario: registrar del plan o del catálogo, ver el día, borrar, navegar por fecha.
- ✅ `dotnet build` sin warnings; 259/259 pruebas en verde.
- ⏳ Pendiente (Fase 6): resiliencia de IA (D-09, D-20), ahora ADR-16.
