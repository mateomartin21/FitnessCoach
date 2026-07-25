namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Una alternativa a una porción del plan: el mismo papel nutricional, en la
    /// cantidad que hace falta para aportar lo mismo.
    ///
    /// "En vez de 150 g de pollo, 190 g de merluza" — eso es lo que ofrece un
    /// nutricionista con la tabla de intercambios, y esto lo calcula solo.
    /// </summary>
    public sealed class SustitucionAlimento
    {
        public required Alimento Alimento { get; init; }
        public required double Gramos { get; init; }

        public MacrosPorcion Macros => Alimento.MacrosPara(Gramos);

        /// <summary>Cómo se lee en la lista de alternativas.</summary>
        public string Descripcion => $"{Gramos:0} g de {Alimento.Nombre}";
    }
}
