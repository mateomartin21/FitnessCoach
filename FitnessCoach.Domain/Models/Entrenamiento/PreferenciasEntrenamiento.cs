using FitnessCoach.Domain.Catalogos;

namespace FitnessCoach.Domain.Models.Entrenamiento
{
    /// <summary>
    /// Con qué puede entrenar el usuario. Filtra el catálogo antes de que la estrategia
    /// componga la rutina, igual que <c>PreferenciasAlimentarias</c> hace con la comida.
    ///
    /// Sin equipo elegido no se filtra nada: un perfil recién creado tiene que ver una
    /// rutina completa, no una vacía.
    /// </summary>
    public class PreferenciasEntrenamiento
    {
        /// <summary>Grupos de <see cref="EquipoEntrenamiento"/> con los que cuenta.</summary>
        public List<string> EquipoDisponible { get; set; } = new();

        /// <summary>
        /// Cambios puntuales: slug que eligió la estrategia → slug que prefiere el usuario.
        ///
        /// La clave es siempre el ejercicio **original**, no el que se está viendo. Si no,
        /// cambiar dos veces la misma fila encadenaría A→B y B→C, y bastaría con que la
        /// estrategia dejara de elegir A para que el cambio se perdiera.
        /// </summary>
        public Dictionary<string, string> Sustituciones { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public bool SinRestricciones => EquipoDisponible.Count == 0;

        public bool Permite(Ejercicio ejercicio)
        {
            ArgumentNullException.ThrowIfNull(ejercicio);

            if (SinRestricciones) return true;

            // "other" son tres ejercicios sueltos que no encajan en ningún grupo: se dejan
            // pasar siempre antes que perderlos por no tener dónde clasificarlos.
            if (string.Equals(ejercicio.Equipo, "other", StringComparison.OrdinalIgnoreCase))
                return true;

            return EquipoEntrenamiento.EquiposDe(EquipoDisponible).Contains(ejercicio.Equipo ?? string.Empty);
        }
    }
}
