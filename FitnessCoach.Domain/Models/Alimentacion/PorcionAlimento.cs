namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Un alimento concreto con la cantidad que toca comer.
    ///
    /// Reemplaza al texto suelto que había antes ("180g pechuga de pollo a la plancha"):
    /// con el alimento del catálogo detrás, los macros de la comida se calculan en vez
    /// de escribirse a mano, y se puede ofrecer un sustituto equivalente.
    /// </summary>
    public sealed class PorcionAlimento
    {
        public required Alimento Alimento { get; init; }

        public required double Gramos { get; init; }

        public MacrosPorcion Macros => Alimento.MacrosPara(Gramos);

        /// <summary>
        /// Cómo se lee en el plan. Lleva la medida casera entre paréntesis porque
        /// nadie tiene una balanza en la mano cada vez que come.
        /// </summary>
        public string Descripcion
        {
            get
            {
                var texto = $"{Gramos:0} g de {Alimento.Nombre}";
                return string.IsNullOrWhiteSpace(Alimento.DescripcionPorcion)
                    ? texto
                    : $"{texto} ({MedidaCasera()})";
            }
        }

        /// <summary>
        /// Ajusta la medida casera a la cantidad real. Si la porción del plan es el
        /// doble de la típica, no sirve decir "1 pechuga mediana".
        /// </summary>
        private string MedidaCasera()
        {
            if (Alimento.PorcionTipicaG <= 0) return Alimento.DescripcionPorcion;

            var proporcion = Gramos / Alimento.PorcionTipicaG;

            return proporcion switch
            {
                < 0.4 => $"un poco menos de {Alimento.DescripcionPorcion}",
                < 0.8 => $"algo menos de {Alimento.DescripcionPorcion}",
                <= 1.25 => Alimento.DescripcionPorcion,
                < 1.75 => $"algo más de {Alimento.DescripcionPorcion}",
                _ => $"cerca del doble de {Alimento.DescripcionPorcion}"
            };
        }
    }
}
