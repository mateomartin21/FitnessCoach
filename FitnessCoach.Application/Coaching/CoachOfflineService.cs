using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// El Lobo sin conexión. Es el proveedor de respaldo: no habla con ninguna IA, así
    /// que funciona aunque no haya internet. Razona por reglas simples sobre la pregunta
    /// del usuario y responde en la voz del Lobo.
    ///
    /// Nunca lanza: es el último de la cadena y su razón de ser es que siempre haya una
    /// respuesta. Sus consejos son generales —no reemplazan a la IA real—, pero son
    /// útiles y en personaje, que es justo lo que la visión pide cuando "se va la señal".
    /// </summary>
    public class CoachOfflineService : IProveedorIA
    {
        public string Nombre => "Offline";
        public bool EsRespaldo => true;

        public Task<string> GenerarAsync(ConsultaIA consulta, CancellationToken cancellationToken = default)
        {
            var pregunta = (consulta.Mensaje ?? string.Empty).ToLowerInvariant();
            return Task.FromResult(Aviso + ResponderSegunTema(pregunta));
        }

        // Se aclara de entrada que ahora está en modo sin conexión, para no hacer pasar
        // un consejo general por una respuesta a medida.
        private const string Aviso =
            "Ando sin señal para pensarlo a fondo, campeón, así que te dejo lo esencial: ";

        private static string ResponderSegunTema(string pregunta)
        {
            if (Menciona(pregunta, "proteina", "comer", "comida", "dieta", "aliment", "caloria", "macro"))
                return "reparte la proteína en todas tus comidas y que la mitad del plato sean verduras. " +
                       "Tu plan de alimentación ya tiene los números y hasta los reemplazos; síguelo y vas bien.";

            if (Menciona(pregunta, "descans", "dormir", "sueno", "recupera", "cansad"))
                return "el músculo se construye descansando, no solo entrenando. Duerme tus siete u ocho " +
                       "horas y respeta los días de descanso: no es flojera, es parte del plan.";

            if (Menciona(pregunta, "motiva", "ganas", "animo", "abandonar", "rendir", "dificil"))
                return "nadie llega por un día perfecto, sino por muchos días normales sin faltar. Hoy haz " +
                       "lo que puedas, pero hazlo. Mañana te vas a alegrar de no haber parado.";

            if (Menciona(pregunta, "dolor", "lesion", "molesta", "duele", "lastim"))
                return "si algo duele de verdad, no lo fuerces: detén ese ejercicio y, si sigue, consulta a un " +
                       "profesional. Entrenar con dolor no es aguante, es apresurar una lesión.";

            if (Menciona(pregunta, "rutina", "ejercicio", "entrena", "peso", "serie", "repetic"))
                return "constancia y buena técnica antes que cargar de más. Tu rutina ya está armada para tu " +
                       "objetivo; complétala y sube el peso poco a poco cuando las últimas repeticiones " +
                       "te salgan sobradas.";

            return "mantén la constancia con tu rutina y tu plan de comidas, que ya están hechos para tu " +
                   "objetivo. Cuando vuelva la señal te ayudo con lo puntual que necesites.";
        }

        private static bool Menciona(string texto, params string[] claves) =>
            claves.Any(texto.Contains);
    }
}
