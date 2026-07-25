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
            "Ando sin señal para pensarlo a fondo, campeon, asi que te tiro lo esencial: ";

        private static string ResponderSegunTema(string pregunta)
        {
            if (Menciona(pregunta, "proteina", "comer", "comida", "dieta", "aliment", "caloria", "macro"))
                return "apunta a repartir la proteina en todas las comidas y a que la mitad del plato sean " +
                       "verduras. Tu plan de alimentacion ya tiene los numeros y hasta los reemplazos; " +
                       "seguilo y vas bien.";

            if (Menciona(pregunta, "descanso", "dormir", "sueno", "recupera", "cansado"))
                return "el musculo se construye descansando, no solo entrenando. Dormi tus siete u ocho " +
                       "horas y respeta los dias de descanso: no es vagancia, es parte del plan.";

            if (Menciona(pregunta, "motiva", "ganas", "animo", "abandonar", "rendir", "dificil"))
                return "nadie llega por un dia perfecto, sino por muchos dias normales sin faltar. Hoy hace " +
                       "lo que puedas, pero hacelo. Manana te vas a alegrar de no haber parado.";

            if (Menciona(pregunta, "dolor", "lesion", "molesta", "duele", "lastim"))
                return "si algo duele de verdad, no lo fuerces: para ese ejercicio y, si sigue, consulta a un " +
                       "profesional. Entrenar con dolor no es aguante, es apurar una lesion.";

            if (Menciona(pregunta, "rutina", "ejercicio", "entrena", "peso", "serie", "repetic"))
                return "constancia y buena tecnica antes que cargar de mas. Tu rutina ya esta armada para tu " +
                       "objetivo; cumplila completa y subi el peso de a poco cuando las ultimas repeticiones " +
                       "te salgan sobradas.";

            return "manteni la constancia con tu rutina y tu plan de comidas, que ya estan hechos para tu " +
                   "objetivo. Cuando vuelva la senal te ayudo con lo puntual que necesites.";
        }

        private static bool Menciona(string texto, params string[] claves) =>
            claves.Any(texto.Contains);
    }
}
