# ADR-21: Entrada sin sesión, centro de ajustes y el equipo del usuario como filtro de la rutina

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 31/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** el ADR-13 trajo el catálogo real de ejercicios y el Strategy que compone la rutina desde él; el ADR-14 y el ADR-15 hicieron lo propio con la comida, y ahí sí agregaron preferencias y sustituciones. Este ADR corrige esa asimetría y reordena la entrada a la app, que el ADR-10 dejó montada sobre el layout completo.

---

## Contexto

El roadmap cerró en la Fase 10 con el producto completo. Usarlo de punta a punta destapó dos cosas que ninguna prueba
podía señalar porque no son fallos: son decisiones que nunca se tomaron.

### El catálogo estaba desaprovechado

La base tiene **1323 ejercicios repartidos en 19 grupos musculares**. Una rutina usa:

| Objetivo | Días | Ejercicios del catálogo | % del catálogo |
|----------|------|-------------------------|----------------|
| Perder peso | 3 | 16 | 1.2 % |
| Recomposición | 4 | 18 | 1.4 % |
| Ganar músculo | 5 | 23 | 1.7 % |

Y esos ejercicios **no cambian nunca**. `EstrategiaRutinaBase.OrdenEstable(slug)` se siembra con el `Id` del perfil, lo
que garantiza que la rutina sea estable entre recargas —correcto y deliberado— pero también que sea **estable de por
vida**. Un usuario ve los mismos 16 ejercicios el primer día y el año siguiente.

Peor que la repetición: `EquiposPreferidos` estaba **fijo en cada estrategia**. La de perder peso prefiere peso corporal
y mancuernas; la de ganar músculo prefiere barra. Nada de eso pregunta con qué cuenta la persona. A quien entrena en su
cuarto con un par de bandas se le prescribía sentadilla con barra, y no tenía forma de cambiarlo.

La comparación con la comida deja la asimetría a la vista: ahí sí hay `PreferenciasAlimentarias` (dietas y vetos) y
`CalculadorEquivalencias` (sustituciones por macros). En entrenamiento no había nada.

### La entrada sin cuenta no tenía sentido

La raíz mostraba la portada de marketing con la barra de navegación completa. Los siete enlaces de esa barra
—Perfil, Dieta, Diario, Rutina, Progreso, Logros, Koda— llevan a controladores con `[Authorize]`, así que para alguien
sin cuenta **todos rebotaban al login**. Una navegación en la que ningún enlace funciona es peor que no tener navegación.

---

## Decisión

### 1. La bienvenida presenta a Koda y manda al login

`HomeController.Index` bifurca por sesión: sin autenticar devuelve la vista `Bienvenida`, con sesión devuelve la portada
de siempre. La bienvenida es una sola pantalla —marca, Koda con un globo de diálogo, y la pregunta— con dos salidas:
entrar o crear cuenta.

La portada de marketing no se pierde: queda en `Home/Conoce`, accesible sin cuenta desde el pie de la bienvenida. Su
contenido es el mismo archivo; lo que cambia según la sesión es el layout y a dónde apuntan sus botones.

### 2. Un layout sin navegación para todo lo que se ve sin cuenta

`_LayoutLimpio.cshtml` no trae barra ni pie. Lo usan bienvenida, login, registro y la portada pública. La regla que lo
justifica es simple: **no se enseña navegación que la persona no puede usar**. La portada pública recupera lo mínimo con
una barra propia de dos botones (entrar / crear cuenta).

Efecto colateral aprovechado: el script de mostrar/ocultar contraseña estaba duplicado literal en login y registro. Al
tocar ambas vistas salió a la parcial `_MostrarContrasena`.

### 3. Un centro de ajustes, separado del perfil

`AjustesController` reúne lo que se configura una vez y manda en toda la app: la cuenta (correo y cambio de contraseña),
el calendario, la alimentación y el equipo. El perfil se queda solo con lo que alimenta los cálculos —edad, peso,
estatura, objetivo—.

**La zona horaria se mudó del perfil a ajustes.** El ADR-20 la había metido en el formulario de perfil, donde no encajaba
conceptualmente (no es un dato del cuerpo) y donde además su texto de ayuda quedaba cortado por el botón de guardar.

El cambio de contraseña llama a `RefreshSignInAsync` después de `ChangePasswordAsync`: cambiar la contraseña rota el
sello de seguridad e invalida la cookie, y sin refrescarla el usuario se encontraría deslogueado al navegar. Los errores
de Identity se traducen al español, porque el resto de la app no habla inglés. La acción va con el mismo rate limiter que
el login: se puede probar la contraseña actual por fuerza bruta desde una sesión abierta.

### 4. El equipo del usuario filtra el catálogo, y es un filtro distinto del de la estrategia

`PreferenciasEntrenamiento.EquipoDisponible` vive en el perfil como objeto de valor (`OwnsOne`), igual que
`PreferenciasAlimentarias`. La estrategia lo recibe por constructor y lo aplica **antes** de su propia preferencia.

Los dos filtros son cosas distintas y por eso conviven:

- **Disponibilidad del usuario** — duro. Lo que no tiene, no se le prescribe.
- **`EquiposPreferidos` de la estrategia** — blando. Sesga la elección (principiante antes con mancuerna que con barra),
  pero no descarta.

Ambos comparten una salvaguarda: **si el filtro deja un bloque sin ningún candidato, se ignora**. Es mejor prescribir un
ejercicio con otro equipo que devolver un día de pierna vacío. El caso real: alguien marca solo peso corporal y el
catálogo no tiene nada de ese grupo sin material.

### 5. Los doce equipos del catálogo se agrupan en seis opciones

La columna `Equipo` tiene doce valores crudos (`bodyweight`, `dumbbell`, `cable`, `barbell`, `lever`, `band`, `smith`,
`kettlebell`, `ez-bar`, `sled`, `machine`, `other`). Nadie sabe si tiene un "lever" o un "smith". `EquipoEntrenamiento`
los agrupa en seis opciones reconocibles —peso corporal, mancuernas, bandas, barra, poleas, máquinas— y el usuario marca
las que le aplican.

`other` (tres ejercicios sueltos) **se deja pasar siempre**: no vale la pena perderlos por no tener dónde clasificarlos.

Hay una prueba que carga el catálogo real y verifica que **todo valor de `Equipo` cae en algún grupo**. Si mañana la
semilla trae un equipo nuevo, el filtro lo excluiría en silencio de todas las rutinas; esa prueba lo convierte en un
fallo visible.

### 6. Sin equipo marcado no se filtra nada

Un perfil recién creado tiene la lista vacía, y eso significa "todo entra", no "nada entra". La alternativa —obligar a
elegir equipo antes de ver una rutina— pone un trámite entre el registro y el primer valor que la app entrega.

---

## Alternativas consideradas

**Aleatorizar la rutina en cada carga.** Resolvía la repetición de un plumazo y era la respuesta obvia a "siempre veo lo
mismo". Se descartó: una rutina que cambia al refrescar la página es inservible para seguir un progreso, y el ADR-13 ya
había tomado esa decisión por buenas razones. El problema no era que fuera estable, era que no fuera *elegible*.

**Rotar la semilla por semana.** Habría dado variedad automática sin intervención. Se descartó por lo mismo: nadie pidió
que su rutina cambie sola, y romper récords personales exige repetir el ejercicio. Queda anotada como idea si algún día
existe una noción de "bloque de entrenamiento".

**Exponer los doce equipos crudos.** Menos código y ningún mapeo que mantener. Se descartó porque la interfaz habría
pedido al usuario distinguir `lever` de `machine`, que es vocabulario de la base de datos, no del gimnasio.

**Filtrar en la consulta SQL en vez de en memoria.** Más eficiente en teoría. Se descartó porque el catálogo está en
caché desde la Fase 10 (`RepositorioEjerciciosEnCache`, 12 h) y filtrar una lista ya en memoria no toca la base; hacerlo
en SQL habría anulado la caché.

**Meter los ajustes dentro del perfil.** Una pantalla menos. Se descartó porque el perfil ya tenía el problema opuesto
—demasiadas cosas distintas en un formulario— y porque la contraseña no pertenece al mismo formulario que la estatura.

---

## Consecuencias

**A favor**

- El catálogo pasa de decorativo a útil: dos personas con el mismo objetivo y distinto equipo ven rutinas distintas.
- La primera visita tiene una sola pregunta clara en vez de una navegación que no lleva a ningún lado.
- El perfil queda con una sola responsabilidad, y la zona horaria en el lugar donde uno la buscaría.
- La entidad `PreferenciasEntrenamiento` deja el hueco listo para las sustituciones puntuales, que es lo que falta.

**En contra / pendiente**

- **Cambiar un ejercicio suelto por otro relacionado sigue sin existir.** El equipo filtra en bloque; no permite decir
  "este me lastima el hombro, dame otro de pecho". Es el siguiente paso de la fase y necesita persistir la elección, algo
  que hoy no ocurre porque la rutina se genera al vuelo y no se guarda. Queda como **D-36**.
- Marcar equipo cambia la rutina completa de golpe. Para alguien que ya venía siguiéndola es un cambio brusco; se avisa
  en el texto, pero no hay confirmación.
- La migración agregó la columna con `defaultValue: "[]"` corrigiendo a mano lo que EF generó (`""`). La columna se lee
  con `JsonSerializer`, que revienta con la cadena vacía: dejar el valor generado habría tumbado todos los perfiles
  existentes. Es el mismo arreglo que ya se había hecho en la migración de `PreferenciasAlimentarias`, y ya van dos veces:
  queda anotado como **D-37**.
