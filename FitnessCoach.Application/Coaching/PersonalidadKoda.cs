using System.Text.RegularExpressions;
using FitnessCoach.Domain.Models.Coaching;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// Quién es Koda y cómo habla. Vive acá, separado de cualquier adaptador de IA,
    /// porque la personalidad es del producto (05-VISION-PRODUCTO): cambiar de proveedor
    /// no debe tocar la personalidad, ni al revés (D-20).
    ///
    /// Arma el prompt que se le manda a un modelo de lenguaje y guarda las respuestas
    /// que da Koda cuando la IA no está disponible, en su misma voz.
    /// </summary>
    public static class PersonalidadKoda
    {
        /// <summary>
        /// Construye el prompt a partir del contexto del usuario y su pregunta. Es el
        /// único lugar donde se define el tono y las reglas; los proveedores solo lo
        /// mandan tal cual.
        /// </summary>
        /// <param name="historial">
        /// Los últimos turnos de la charla, del más viejo al más nuevo, para que Koda
        /// pueda retomar el hilo. Vacío o null: responde como si fuera la primera vez.
        /// </param>
        public static string ConstruirPrompt(
            string mensaje, string contextoPerfil, IReadOnlyList<MensajeCoach>? historial = null)
        {
            return $@"Eres Koda: un entrenador personal con experiencia, de la vieja escuela, que conoce a
fondo a su pupilo y lo trata como a alguien de confianza. Eres motivador pero directo, nunca
condescendiente. Tienes carácter: celebras el esfuerzo, no dejas pasar las excusas y hablas
claro. Le dices 'campeón' o por su nombre, y no das discursos genéricos.

Esto es TODO lo que el sistema ya sabe de tu pupilo (su plan, su rutina, su diario y sus
números son reales, generados por la app):
{contextoPerfil}
{TranscripcionDe(historial)}
REGLAS QUE NO PUEDES ROMPER:
1. Solo puedes recomendar alimentos y ejercicios que aparezcan arriba: en su plan, en su rutina
   o en la lista de alimentos disponibles. NUNCA inventes alimentos, ejercicios, marcas,
   suplementos ni rutinas que no estén en ese contexto. Si algo no está, dilo con honestidad y
   remítelo a su plan o su rutina de la app.
2. Cuando te pregunte por su progreso, su dieta o su entrenamiento, responde con SUS datos
   concretos de arriba (su peso, sus récords, lo que comió hoy, las comidas de su plan), no con
   generalidades que servirían para cualquiera.
3. Eres un coach, no un médico: ante dolor, lesión o temas de salud, recomienda consultar a un
   profesional.

FORMATO (esto se lee en un celular, no en un libro):
- Español de México, de 'tú'. Nunca 'vos' ni conjugaciones como 'tenés' o 'podés'.
- De 2 a 4 párrafos CORTOS (dos o tres renglones cada uno), separados entre sí por
  una línea en blanco. Nunca un solo bloque largo: en una pantalla chica no se lee.
- Resalta con **doble asterisco** lo que no se puede perder de vista: un número, un
  alimento, un ejercicio o la acción concreta. Máximo 3 por respuesta; si resaltas todo,
  no resaltas nada.
- Si enumeras, usa hasta 4 viñetas que empiecen con '- ', una por renglón y de una sola
  línea cada una.
- Nada más de markdown: sin títulos, sin #, sin tablas, sin `código`.
- Cierra con UNA frase que le diga qué hacer ahora.

Pregunta de tu pupilo: {mensaje}";
        }

        /// <summary>
        /// La charla previa, lista para pegar en el prompt. Va ANTES de las reglas a
        /// propósito: lo que escribió el usuario es texto que no controlamos, y las
        /// reglas pesan más cuando son lo último que el modelo lee.
        /// </summary>
        private static string TranscripcionDe(IReadOnlyList<MensajeCoach>? historial)
        {
            if (historial is null || historial.Count == 0)
                return string.Empty;

            var lineas = historial
                .Where(m => !string.IsNullOrWhiteSpace(m.Texto))
                // En una linea: un salto dentro de un turno se leeria como un turno nuevo.
                .Select(m => $"{(m.EsDeKoda ? "Koda" : "Pupilo")}: {EnUnaLinea(m.Texto)}");

            return $@"
Lo que ya se dijeron en esta conversación (lo más viejo primero). Retoma el hilo en vez de
volver a presentarte o repetir lo que ya le dijiste:
{string.Join("\n", lineas)}
";
        }

        // Cualquier racha de espacios, saltos o tabulaciones queda en un solo espacio:
        // reemplazar \r y \n por separado dejaba dos espacios en cada "\r\n".
        private static string EnUnaLinea(string texto) =>
            Regex.Replace(texto, @"\s+", " ").Trim();

        /// <summary>
        /// El pedido con que Koda analiza un aspecto del usuario, para las tarjetas de
        /// análisis fuera del chat. Se resuelve contra el mismo contexto rico, así que la
        /// respuesta usa los datos reales de la persona.
        ///
        /// Cada pedido cierra pidiendo dos párrafos: la tarjeta es chica y ahí una
        /// respuesta de cuatro bloques obliga a desplazarse dentro de la propia tarjeta.
        /// </summary>
        public static string PedidoDeAnalisis(string aspecto) => (aspecto?.ToLowerInvariant() switch
        {
            "dieta" =>
                "Revisa mi plan de alimentación y lo que registré en el diario de hoy. Dime si voy " +
                "encaminado y el ajuste más importante, usando solo alimentos de mi plan o de la lista " +
                "disponible.",
            "rutina" =>
                "Revisa mi rutina actual según mi objetivo. Dime si está bien encarada y un consejo " +
                "concreto para sacarle más provecho, refiriéndote a los ejercicios de mi rutina.",
            "semana" =>
                "Haz un resumen de mi última semana con los datos de la sección ESTA SEMANA: cuántas veces " +
                "entrené, cómo viene mi racha y cómo se movió mi peso. Nárralo en tu voz, reconoce lo que " +
                "hice bien y márcame una cosa concreta para la semana que viene.",
            _ =>
                "Analiza mi progreso reciente: mi peso, mis récords y lo que vengo comiendo. Dime cómo voy " +
                "y el ajuste más importante que harías ahora mismo."
        }) + " Respóndeme en 2 párrafos cortos, con el dato clave en **negritas**.";

        /// <summary>
        /// Lo que dice Koda cuando ningún proveedor de IA pudo responder. En su voz, sin
        /// exponer el error técnico: el usuario ve al personaje encogiéndose de hombros,
        /// no un stack trace (05-VISION-PRODUCTO).
        /// </summary>
        public const string RespuestaSinSenal =
            "Se me fue la señal un momento, campeón. No pude pensar bien tu respuesta ahora mismo.\n\n" +
            "Dame unos segundos y **vuelve a preguntarme**; mientras tanto, si es una duda de tu rutina o " +
            "tu plan, revísalos que ahí está casi todo lo que necesitas.";
    }
}
