using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models.Coaching
{
    /// <summary>
    /// Un turno de la conversación con Koda: lo que preguntó el usuario o lo que
    /// contestó él. Se guarda para que el chat siga ahí al volver a entrar y para
    /// que Koda tenga de qué acordarse.
    /// </summary>
    /// <remarks>
    /// Deliberadamente NO es una colección owned de <c>UsuarioPerfil</c>, a diferencia
    /// del diario o los récords. EF carga las colecciones owned siempre, y el perfil se
    /// lee en cada pantalla: la charla entera viajaría en cada página aunque solo se use
    /// en el chat. Con una base remota eso es un viaje de red de más por pantalla.
    /// Vive aparte y se lee solo cuando alguien abre la conversación.
    /// </remarks>
    public class MensajeCoach
    {
        /// <summary>Máximo de mensajes que se conservan por usuario.</summary>
        /// <remarks>
        /// Sin tope la conversación crece sin límite en una tabla que nadie poda.
        /// Cuarenta son veinte idas y vueltas: alcanza de sobra para retomar el hilo
        /// y para lo que se le manda al modelo como memoria.
        /// </remarks>
        public const int MaximoGuardados = 40;

        /// <summary>Cuánto del historial viaja en el prompt como memoria de Koda.</summary>
        /// <remarks>
        /// Seis mensajes son tres intercambios. Más que eso encarece cada consulta sin
        /// mejorar la respuesta, y los proveedores gratuitos tienen cupo por minuto.
        /// </remarks>
        public const int MensajesDeMemoria = 6;

        /// <summary>Largo máximo de un mensaje, el mismo que acepta el chat.</summary>
        public const int TextoLargoMaximo = 4000;

        public int Id { get; set; }

        /// <summary>De quién es la conversación.</summary>
        public int UsuarioPerfilId { get; set; }

        /// <summary>Siempre en UTC. La conversión a hora local se hace al mostrar.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>true si lo dijo Koda; false si lo escribió el usuario.</summary>
        public bool EsDeKoda { get; set; }

        [StringLength(TextoLargoMaximo)]
        public string Texto { get; set; } = string.Empty;

        public static MensajeCoach DelUsuario(string texto, DateTime fechaUtc) =>
            new() { Texto = Recortar(texto), Fecha = fechaUtc, EsDeKoda = false };

        public static MensajeCoach DeKoda(string texto, DateTime fechaUtc) =>
            new() { Texto = Recortar(texto), Fecha = fechaUtc, EsDeKoda = true };

        // La respuesta de un modelo de lenguaje no tiene largo garantizado: si se pasa
        // del maximo de la columna, la insercion falla y se pierde el intercambio entero.
        private static string Recortar(string texto)
        {
            var limpio = (texto ?? string.Empty).Trim();
            return limpio.Length <= TextoLargoMaximo ? limpio : limpio[..TextoLargoMaximo];
        }
    }
}
