using System.Text;
using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// Junta todo lo que el sistema sabe del usuario en un texto que el Lobo lee antes
    /// de responder. Cada bloque se arma por separado y protegido: si uno falla (por
    /// ejemplo, un perfil sin datos válidos para calcular el plan), se omite ese bloque
    /// en vez de dejar al coach sin ningún contexto.
    /// </summary>
    public class ArmadorContextoCoach : IArmadorContextoCoach
    {
        private readonly IGeneradorAlimentacion _alimentacion;
        private readonly IGeneradorRutinas _rutinas;
        private readonly IServicioRecords _records;
        private readonly IServicioDiario _diario;
        private readonly IRepositorioAlimentos _catalogo;

        public ArmadorContextoCoach(
            IGeneradorAlimentacion alimentacion,
            IGeneradorRutinas rutinas,
            IServicioRecords records,
            IServicioDiario diario,
            IRepositorioAlimentos catalogo)
        {
            _alimentacion = alimentacion;
            _rutinas = rutinas;
            _records = records;
            _diario = diario;
            _catalogo = catalogo;
        }

        public string Construir(UsuarioPerfil usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var sb = new StringBuilder();

            Perfil(sb, usuario);
            Progreso(sb, usuario);
            EstaSemana(sb, usuario);
            Records(sb, usuario);
            PlanDeComidas(sb, usuario);
            DiarioDeHoy(sb, usuario);
            Rutina(sb, usuario);
            CatalogoParaAnclar(sb);

            return sb.ToString();
        }

        private static void Perfil(StringBuilder sb, UsuarioPerfil u)
        {
            sb.AppendLine("== PERFIL ==");
            sb.AppendLine($"Nombre: {u.Nombre}, Edad: {u.Edad}, Peso: {u.PesoKg}kg, Estatura: {u.EstaturaCm}cm.");
            sb.AppendLine($"Objetivo: {u.ObjetivoActual?.Nombre ?? "no definido"}.");

            if (u.Preferencias.DietasSeguidas.Count > 0)
                sb.AppendLine($"Dietas que sigue: {string.Join(", ", u.Preferencias.DietasSeguidas)}.");
            if (u.Preferencias.AlimentosExcluidos.Count > 0)
                sb.AppendLine($"Alimentos que excluye: {string.Join(", ", u.Preferencias.AlimentosExcluidos)}.");
            sb.AppendLine();
        }

        private static void Progreso(StringBuilder sb, UsuarioPerfil u)
        {
            if (u.HistorialProgreso.Count == 0) return;

            var ultimos = u.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .Take(3)
                .Select(r => $"{r.PesoKg}kg ({r.Fecha:dd/MM})");

            sb.AppendLine("== PESO RECIENTE ==");
            sb.AppendLine(string.Join(" · ", ultimos));
            sb.AppendLine();
        }

        /// <summary>
        /// El pulso de los últimos 7 días: entrenamientos hechos, racha y cómo se movió
        /// el peso. Alimenta el resumen semanal y da contexto al resto de las respuestas.
        /// Se cuenta en la zona del usuario (D-25).
        /// </summary>
        private static void EstaSemana(StringBuilder sb, UsuarioPerfil u)
        {
            try
            {
                var zona = ZonaHorariaUsuario.De(u);
                var ahora = ZonaHorariaUsuario.Ahora(zona);
                var hoyLocal = DateOnly.FromDateTime(ahora);
                var desde = ahora.AddDays(-7);

                var entrenosSemana = u.EntrenamientosCompletados
                    .Where(e => ZonaHorariaUsuario.ALocal(e.Fecha, zona) >= desde)
                    .OrderByDescending(e => e.Fecha)
                    .ToList();

                var racha = CalculadorRachas.Calcular(
                    u.EntrenamientosCompletados.Select(e => ZonaHorariaUsuario.ALocal(e.Fecha, zona)), hoyLocal);

                var pesosSemana = u.HistorialProgreso
                    .Where(r => ZonaHorariaUsuario.ALocal(r.Fecha, zona) >= desde)
                    .OrderBy(r => r.Fecha)
                    .ToList();

                // Sin actividad ni racha ni pesos de la semana, el bloque no aporta nada.
                if (entrenosSemana.Count == 0 && racha.Actual == 0 && pesosSemana.Count == 0) return;

                sb.AppendLine("== ESTA SEMANA (ultimos 7 dias) ==");
                sb.AppendLine($"Entrenamientos: {entrenosSemana.Count}. " +
                              $"Racha actual: {racha.Actual} dia(s), mejor racha {racha.MasLarga}.");

                if (entrenosSemana.Count > 0)
                    sb.AppendLine("Hizo: " + string.Join("; ",
                        entrenosSemana.Select(e => $"{e.NombreRutina} ({e.DuracionMinutos}min, {ZonaHorariaUsuario.ALocal(e.Fecha, zona):dd/MM})")));

                if (pesosSemana.Count >= 2)
                {
                    var delta = pesosSemana[^1].PesoKg - pesosSemana[0].PesoKg;
                    var signo = delta > 0 ? "+" : "";
                    sb.AppendLine($"Peso: de {pesosSemana[0].PesoKg}kg a {pesosSemana[^1].PesoKg}kg " +
                                  $"({signo}{delta:0.#}kg en la semana).");
                }
                sb.AppendLine();
            }
            catch { /* se omite */ }
        }

        private void Records(StringBuilder sb, UsuarioPerfil u)
        {
            if (string.IsNullOrWhiteSpace(u.IdentityUserId)) return;

            try
            {
                var records = _records.ObtenerTodos(u.IdentityUserId);
                if (records.Count == 0) return;

                sb.AppendLine("== RECORDS PERSONALES ==");
                foreach (var r in records.Take(10))
                    sb.AppendLine($"{r.EjercicioNombre}: {r.PesoKg}kg x{r.Repeticiones}");
                sb.AppendLine();
            }
            catch { /* sin records legibles: se omite el bloque */ }
        }

        private void PlanDeComidas(StringBuilder sb, UsuarioPerfil u)
        {
            if (u.ObjetivoActual is null) return;

            try
            {
                var plan = _alimentacion.GenerarPlanPara(u);

                sb.AppendLine("== PLAN DE ALIMENTACION (generado por el sistema) ==");
                sb.AppendLine($"Objetivo diario: {plan.Objetivos.Calorias} kcal · " +
                              $"Proteina {plan.Objetivos.ProteinaG}g, Carbohidratos {plan.Objetivos.CarbohidratoG}g, Grasas {plan.Objetivos.GrasaG}g.");

                foreach (var comida in plan.Comidas)
                    sb.AppendLine($"- {comida.NombreComida} ({comida.Hora}), {comida.Calorias} kcal: " +
                                  $"{string.Join("; ", comida.Alimentos)}.");

                sb.AppendLine("(Cada alimento del plan ya tiene reemplazos equivalentes en macros.)");
                sb.AppendLine();
            }
            catch { /* perfil sin datos válidos para el plan: se omite */ }
        }

        private void DiarioDeHoy(StringBuilder sb, UsuarioPerfil u)
        {
            try
            {
                // "Hoy" es el día del usuario, no el de UTC (D-25).
                var hoy = ZonaHorariaUsuario.Hoy(ZonaHorariaUsuario.De(u));
                var resumen = _diario.ResumenDelDia(u, hoy);

                sb.AppendLine("== DIARIO DE HOY (lo que realmente comio) ==");
                if (resumen.SinRegistros)
                {
                    sb.AppendLine("Todavia no registro nada hoy.");
                }
                else
                {
                    sb.AppendLine($"Lleva {resumen.CaloriasConsumidas}/{resumen.Objetivo.Calorias} kcal, " +
                                  $"proteina {resumen.ProteinaConsumidaG}/{resumen.Objetivo.ProteinaG}g.");
                    sb.AppendLine("Comio: " + string.Join("; ",
                        resumen.Registros.Select(r => $"{r.Gramos:0}g de {r.AlimentoNombre}")));
                }
                sb.AppendLine();
            }
            catch { /* se omite */ }
        }

        private void Rutina(StringBuilder sb, UsuarioPerfil u)
        {
            if (u.ObjetivoActual is null) return;

            try
            {
                var rutina = _rutinas.GenerarRutinaParaObjetivo(u.ObjetivoActual, u.Id);

                sb.AppendLine($"== RUTINA (generada por el sistema, nivel {rutina.Nivel}) ==");
                foreach (var dia in rutina.Dias)
                {
                    var ejercicios = dia.Ejercicios.Select(e => $"{e.Nombre} {e.Series}x{e.Repeticiones}");
                    sb.AppendLine($"- {dia.NombreDia} ({dia.Enfoque}): {string.Join("; ", ejercicios)}.");
                }
                sb.AppendLine();
            }
            catch { /* se omite */ }
        }

        private void CatalogoParaAnclar(StringBuilder sb)
        {
            try
            {
                var porCategoria = _catalogo.ObtenerTodos()
                    .GroupBy(a => a.Categoria)
                    .OrderBy(g => g.Key);

                sb.AppendLine("== ALIMENTOS DISPONIBLES EN LA APP (unicos que se pueden recomendar) ==");
                foreach (var grupo in porCategoria)
                    sb.AppendLine($"{grupo.Key}: {string.Join(", ", grupo.Select(a => a.Nombre))}.");
                sb.AppendLine();
            }
            catch { /* se omite */ }
        }
    }
}
