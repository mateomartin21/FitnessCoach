namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// Quién es el Lobo Coach y cómo habla. Vive acá, separado de cualquier adaptador
    /// de IA, porque la personalidad es del producto (05-VISION-PRODUCTO): cambiar de
    /// proveedor no debe tocar la personalidad, ni al revés (D-20).
    ///
    /// Arma el prompt que se le manda a un modelo de lenguaje y guarda las respuestas
    /// que da el Lobo cuando la IA no está disponible, en su misma voz.
    /// </summary>
    public static class PersonalidadLoboCoach
    {
        /// <summary>
        /// Construye el prompt a partir del perfil del usuario y su pregunta. Es el único
        /// lugar donde se define el tono; los proveedores solo lo mandan tal cual.
        /// </summary>
        public static string ConstruirPrompt(string mensaje, string contextoPerfil)
        {
            return $@"Eres el Lobo Coach, un entrenador personal experto, motivador y directo.
Tienes acceso al perfil del usuario:
{contextoPerfil}

Responde siempre en espanol, de forma concisa (maximo 3 parrafos), practica y motivadora.
No uses markdown con asteriscos. Usa lenguaje natural y cercano, como un entrenador que
conoce a su pupilo y le habla de igual a igual.

Pregunta del usuario: {mensaje}";
        }

        /// <summary>
        /// Lo que dice el Lobo cuando ningún proveedor de IA pudo responder. En su voz,
        /// sin exponer el error técnico: el usuario ve al personaje encogiéndose de
        /// hombros, no un stack trace (05-VISION-PRODUCTO).
        /// </summary>
        public const string RespuestaSinSenal =
            "Se me fue la senal un momento, campeon. No pude pensar bien tu respuesta ahora mismo. " +
            "Dame unos segundos y volve a preguntarme; mientras tanto, si es una duda de tu rutina o tu " +
            "plan, revisalos que ahi esta casi todo lo que necesitas.";
    }
}
