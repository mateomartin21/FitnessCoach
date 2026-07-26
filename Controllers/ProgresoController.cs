using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Gamificacion;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class ProgresoController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IServicioProgreso _progreso;
        private readonly IServicioEntrenamientos _entrenamientos;
        private readonly IServicioRecords _records;
        private readonly IServicioGamificacion _gamificacion;
        private readonly IGeneradorRutinas _rutinas;

        public ProgresoController(IServicioPerfilUsuario perfiles,
                                  IServicioProgreso progreso,
                                  IServicioEntrenamientos entrenamientos,
                                  IServicioRecords records,
                                  IServicioGamificacion gamificacion,
                                  IGeneradorRutinas rutinas)
        {
            _perfiles = perfiles;
            _progreso = progreso;
            _entrenamientos = entrenamientos;
            _records = records;
            _gamificacion = gamificacion;
            _rutinas = rutinas;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            return View(ArmarVista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarPeso([Bind(Prefix = "Nuevo")] RegistrarPesoViewModel modelo)
        {
            // Nada se guarda hasta saber que los datos son válidos.
            if (!ModelState.IsValid)
                return View("Index", ArmarVista(modelo));

            _progreso.Agregar(IdentityId, modelo.NuevoPeso, modelo.Notas);

            TempData["MensajeProgreso"] = "Registro guardado.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            // El servicio busca dentro del historial del usuario autenticado. Un id ajeno
            // sencillamente no aparece: 404, no 403, para no confirmar que existe (estándar §1.4).
            var registro = _progreso.ObtenerRegistro(IdentityId, id);
            if (registro is null) return NotFound();

            return View(new EditarRegistroViewModel
            {
                Id = registro.Id,
                Fecha = registro.Fecha,
                PesoKg = registro.PesoKg,
                Notas = registro.Notas
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(EditarRegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            if (!_progreso.Editar(IdentityId, modelo.Id, modelo.PesoKg, modelo.Notas))
                return NotFound();

            TempData["MensajeProgreso"] = "Registro actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!_progreso.Eliminar(IdentityId, id))
                return NotFound();

            TempData["MensajeProgreso"] = "Registro eliminado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarEntrenamiento(
            [Bind(Prefix = "NuevoEntrenamiento")] RegistrarEntrenamientoViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", ArmarVista(nuevoEntrenamiento: modelo));

            // Solo se puede marcar como hecho un día real de la propia rutina. Sin esto,
            // el nombre era texto libre: cualquiera anotaba algo inventado y se llevaba el
            // XP y los logros sin haber entrenado. Se valida en el servidor, no solo en la UI.
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            if (!OpcionesRutina(usuario).Contains(modelo.NombreRutina))
            {
                ModelState.AddModelError("NuevoEntrenamiento.NombreRutina",
                    "Elegí un día de tu rutina.");
                return View("Index", ArmarVista(nuevoEntrenamiento: modelo));
            }

            // El logro se desbloquea con el hecho recién registrado: comparo la foto de
            // antes con la de después para avisarle al usuario en el momento.
            var antes = _gamificacion.Estadisticas(IdentityId);
            _entrenamientos.Registrar(IdentityId, modelo.NombreRutina, modelo.DuracionMinutos, modelo.Notas);
            NotificarLogrosNuevos(antes);

            TempData["MensajeProgreso"] = "Entrenamiento registrado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarEntrenamiento(int id)
        {
            if (!_entrenamientos.Eliminar(IdentityId, id))
                return NotFound();

            TempData["MensajeProgreso"] = "Entrenamiento eliminado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarRecord(RegistrarRecordViewModel modelo, string? volverA = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeProgreso"] = "No se pudo registrar la marca: revisá el peso y las repeticiones.";
                return Redirect(DestinoSeguro(volverA));
            }

            var antes = _gamificacion.Estadisticas(IdentityId);
            var resultado = _records.Registrar(IdentityId, modelo.EjercicioSlug, modelo.EjercicioNombre,
                                               modelo.PesoKg, modelo.Repeticiones);
            NotificarLogrosNuevos(antes);

            TempData["MensajeProgreso"] = resultado switch
            {
                { EsNuevoRecord: true, MejoraKg: null } =>
                    $"Primera marca en {modelo.EjercicioNombre}: {modelo.PesoKg} kg × {modelo.Repeticiones}.",
                { EsNuevoRecord: true, MejoraKg: > 0 } =>
                    $"¡Nuevo récord en {modelo.EjercicioNombre}! +{resultado.MejoraKg} kg.",
                { EsNuevoRecord: true } =>
                    $"¡Nuevo récord en {modelo.EjercicioNombre}! Mismo peso, más repeticiones.",
                _ => $"Tu récord en {modelo.EjercicioNombre} sigue siendo " +
                     $"{resultado.Record.PesoKg} kg × {resultado.Record.Repeticiones}."
            };

            return Redirect(DestinoSeguro(volverA));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarRecord(int id)
        {
            if (!_records.Eliminar(IdentityId, id))
                return NotFound();

            TempData["MensajeProgreso"] = "Récord eliminado.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Si el hecho recién registrado desbloqueó algún logro, deja el aviso en la voz
        /// del Lobo para que la pantalla lo muestre. Compara la foto de antes (recibida)
        /// con el estado actual del usuario.
        /// </summary>
        private void NotificarLogrosNuevos(EstadisticasUsuario antes)
        {
            var despues = _gamificacion.Estadisticas(IdentityId);
            var nuevos = EvaluadorLogros.ReciénDesbloqueados(antes, despues);
            if (nuevos.Count == 0) return;

            TempData["LogroDesbloqueado"] = string.Join("\n",
                nuevos.Select(l => $"{l.Icono} ¡Logro desbloqueado: {l.Nombre}! {l.LineaLobo}"));
        }

        /// <summary>
        /// Solo se acepta volver a una ruta propia: un returnUrl sin validar es una
        /// redirección abierta hacia cualquier sitio externo.
        /// </summary>
        private string DestinoSeguro(string? volverA) =>
            !string.IsNullOrWhiteSpace(volverA) && Url.IsLocalUrl(volverA)
                ? volverA
                : Url.Action("Index", "Progreso")!;

        private ProgresoViewModel ArmarVista(
            RegistrarPesoViewModel? nuevo = null,
            RegistrarEntrenamientoViewModel? nuevoEntrenamiento = null)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            return new ProgresoViewModel
            {
                Nuevo = nuevo ?? new RegistrarPesoViewModel { NuevoPeso = usuario.PesoKg },
                Historial = _progreso.ObtenerHistorial(IdentityId).ToList(),
                PesoActual = usuario.PesoKg,
                NuevoEntrenamiento = nuevoEntrenamiento ?? new RegistrarEntrenamientoViewModel(),
                OpcionesRutina = OpcionesRutina(usuario),
                Entrenamientos = _entrenamientos.ObtenerHistorial(IdentityId).ToList(),
                Rachas = _entrenamientos.ObtenerRachas(IdentityId),
                Records = _records.ObtenerTodos(IdentityId).ToList()
            };
        }

        /// <summary>
        /// Los días de la rutina real del usuario, como etiquetas ("Día 1 — Piernas").
        /// Son las únicas opciones válidas para marcar un entrenamiento. La rutina es
        /// determinista (misma semilla, mismos días), así que regenerarla acá coincide
        /// con lo que el usuario ve en su pantalla de rutina. Sin objetivo, no hay días.
        /// </summary>
        private List<string> OpcionesRutina(UsuarioPerfil usuario)
        {
            if (usuario.ObjetivoActual is null) return new();

            var rutina = _rutinas.GenerarRutinaParaObjetivo(usuario.ObjetivoActual, usuario.Id);
            return rutina.Dias.Select(d => $"{d.NombreDia} — {d.Enfoque}").ToList();
        }
    }
}
