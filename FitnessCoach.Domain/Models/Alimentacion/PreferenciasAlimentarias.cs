namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Lo que el usuario puede y no puede comer. Filtra el catálogo antes de que las
    /// estrategias compongan el plan, así que un vegetariano nunca ve pollo — ni como
    /// comida ni como sustituto.
    ///
    /// Dos criterios distintos:
    ///   - <see cref="DietasSeguidas"/>: reglas amplias ("vegano", "sin-gluten"). Un
    ///     alimento pasa solo si cumple **todas** las que el usuario sigue.
    ///   - <see cref="AlimentosExcluidos"/>: vetos puntuales por slug (una alergia, algo
    ///     que no le gusta). Pesan por encima de todo lo demás.
    /// </summary>
    public class PreferenciasAlimentarias
    {
        /// <summary>Etiquetas de dieta que el usuario sigue: "vegetariano", "vegano", "sin-gluten", "sin-lactosa".</summary>
        public List<string> DietasSeguidas { get; set; } = new();

        /// <summary>Slugs de alimentos vetados, por alergia o gusto.</summary>
        public List<string> AlimentosExcluidos { get; set; } = new();

        /// <summary>
        /// Si el alimento se puede incluir en el plan de este usuario.
        /// Un veto puntual manda; después tiene que cumplir todas las dietas seguidas.
        /// </summary>
        public bool Permite(Alimento alimento)
        {
            ArgumentNullException.ThrowIfNull(alimento);

            if (AlimentosExcluidos.Contains(alimento.Slug, StringComparer.OrdinalIgnoreCase))
                return false;

            return DietasSeguidas.All(alimento.Cumple);
        }

        /// <summary>Sin dietas ni vetos: todo entra. Es el estado por defecto de un perfil nuevo.</summary>
        public bool SinRestricciones =>
            DietasSeguidas.Count == 0 && AlimentosExcluidos.Count == 0;
    }
}
