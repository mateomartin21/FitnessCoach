# ADR-22: Migración a PostgreSQL y despliegue en contenedor

| Campo  | Valor          |
|--------|----------------|
| Autor  | Mateo Martin   |
| Fecha  | 31/07/2026     |
| Estado | `Aceptado`     |

> **Relación con ADRs anteriores:** el ADR-07 eligió EF Core + SQL Server y el ADR-09 cerró la deuda de persistencia con el adaptador SQL real. El ADR-06 y el ADR-07 ya anotaban PostgreSQL como alternativa. Este ADR ejecuta ese cambio, y lo hace por una razón que no es técnica.

---

## Contexto

El proyecto llegó al despliegue final con las 12 fases cerradas y la app **lista para desplegar**: migraciones al
arrancar, claves de Data Protection en la base, Dockerfile probado en CI y `/health`. Faltaba una sola cosa: elegir
dónde.

Ahí apareció el problema, y no era de arquitectura sino de mercado:

**No existe SQL Server gratuito sin tarjeta de crédito.**

- **Azure SQL** tiene capa gratuita real, pero exige cuenta de Azure: tarjeta, o verificación académica que puede tardar días.
- **AWS RDS** exige tarjeta en cualquier caso, y su capa gratuita cambió en 2025 a un modelo por créditos.
- Los hostings gratuitos que sí incluyen SQL Server (Somee y similares) tienen límites duros y caídas frecuentes.

En cambio **PostgreSQL sobra**: Neon, Supabase y Render lo ofrecen gratis, sin tarjeta y en minutos.

Esto no fue una sorpresa. Estaba escrito como **D-35** desde la Fase 10, con prioridad *baja* y esta nota: *"ninguno
[de riesgo] a la escala actual; el costo está en migrar el día que haga falta"*. El día llegó, y llegó con horas de
plazo. La ficha subestimaba **cuándo**, no **cuánto**.

## Decisión

**Migrar a PostgreSQL como único motor**, en desarrollo y en producción, y desplegar el contenedor en Render con la
base en Neon.

Un solo motor y no dos: mantener SQL Server para desarrollo y PostgreSQL para producción significa que las diferencias
entre motores aparecen **en producción**, que es exactamente donde no se quieren.

### Qué se tocó

| Cambio | Dónde | Por qué |
|---|---|---|
| `UseSqlServer` → `UseNpgsql` | `Program.cs` | El proveedor de EF. |
| Paquete `Microsoft.EntityFrameworkCore.SqlServer` → `Npgsql.EntityFrameworkCore.PostgreSQL` | dos `.csproj` | — |
| Las 14 migraciones **regeneradas**, no editadas | `Infrastructure/Data/Migrations` | Traían tipos y sintaxis de SQL Server (`nvarchar`, `datetime2`). Editarlas a mano es cómo se rompen los esquemas. |
| `HasFilter("[IdentityUserId] IS NOT NULL")` → comillas dobles | `ApplicationDbContext` | Los corchetes son sintaxis de SQL Server; PostgreSQL usa comillas dobles. |
| Comparaciones de texto bajadas a minúsculas | los dos repositorios SQL de catálogo | **PostgreSQL distingue mayúsculas y SQL Server no.** El índice en memoria de la caché ya comparaba sin distinguirlas, así que el adaptador SQL era el único que cambiaba de comportamiento. |
| `Npgsql.EnableLegacyTimestampBehavior` | `Program.cs` | Ver abajo. |
| Lectura de la variable `PORT` | `Program.cs` | Render y compañía asignan el puerto por entorno y esperan que el proceso escuche ahí; ASP.NET Core no la mira por su cuenta. |

### Sobre las fechas

Npgsql mapea `DateTime` a `timestamp with time zone` y **rechaza cualquier `Kind` que no sea `Utc`**. Es la causa más
común de que una migración a Postgres explote en caliente.

Se activó el modo legacy, que mapea a `timestamp without time zone` — exactamente lo que hacía `datetime2` en SQL
Server. La app guarda instantes UTC desnudos y los marca como UTC al leer (ADR-20, D-25), así que **este modo conserva
la semántica anterior sin cambiar una línea de la lógica de fechas**. La alternativa —adoptar `timestamptz`— habría
obligado a auditar cada escritura, y la fecha del diario **no es un instante sino una etiqueta de día**: convertirla
habría reintroducido el bug del lunes que el ADR-20 documenta.

## Consecuencias

### A favor

- **La arquitectura hexagonal cobró lo que prometía.** El cambio de motor tocó `Program.cs`, dos repositorios y el
  esquema. **`Domain` y `Application` no cambiaron**, y las **363 pruebas siguieron pasando sin tocar ninguna** —
  porque `FitnessCoach.Tests` no referencia `Infrastructure` (ADR-08). Es la primera vez que la inversión de
  dependencias se puso a prueba con un cambio que no era hipotético.
- **Se paga D-35** y se abre el abanico de hosting gratuito.
- Un solo motor en desarrollo y producción: las diferencias entre motores no pueden esconderse hasta el despliegue.

### En contra

- **Se perdió el historial de las 14 migraciones.** Ahora hay una sola, `EsquemaInicialPostgres`. El historial de
  evolución del esquema queda en el registro de git, no en la carpeta de migraciones. Es el precio de regenerar.
- **Ninguna base existente se puede migrar automáticamente.** No hay ruta de SQL Server a PostgreSQL: la base de
  desarrollo se recrea desde cero. Aceptable porque los catálogos se siembran solos y no había datos de producción.
- `.ToLower()` en las comparaciones **no usa el índice** de esas columnas. Irrelevante con 1323 filas y detrás de la
  caché, pero queda anotado: con un catálogo grande habría que usar una columna generada o un índice funcional.
- El plan gratuito de Render **duerme el servicio tras ~15 minutos** sin tráfico, y el primer request tarda ~50 s.

### Verificación

No se dio por bueno por compilar. Se probó contra una base **Neon real y vacía**:

- 15 tablas creadas y catálogos sembrados solos: **1323 ejercicios y 67 alimentos**.
- Registro, alta de perfil y las 9 pantallas respondiendo 200.
- Búsqueda por slug **en mayúsculas**, para confirmar que la diferencia de mayúsculas quedó cubierta.
- Escrituras con fecha (peso y entrenamiento) y su lectura desde la gamificación derivada: nivel y XP correctos.
- **Cero excepciones** en todo el arranque y el recorrido.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| **Azure SQL + Azure for Students** | Conserva SQL Server y no toca migraciones, pero depende de una verificación académica que puede tardar o fallar. Con horas de plazo, el riesgo no lo controlaba el equipo. |
| **AWS (EC2 + RDS)** | Exige tarjeta y su montaje —VPC, security groups, aprovisionamiento— es el más lento de todos. La peor opción para un plazo corto. |
| **Hosting gratuito con SQL Server incluido** | Límites duros y caídas frecuentes; demasiado frágil para una demo en vivo. |
| **SQLite** | Más simple, pero el disco de un contenedor es efímero: cada despliegue borraría las cuentas. |
| **Mantener dos motores** | Las diferencias entre motores aparecerían recién en producción. |
