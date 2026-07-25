namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Un alimento del catálogo, con sus macros por 100 g comestibles.
    ///
    /// Los 100 g son la unidad de referencia de toda la nutrición seria: permite
    /// comparar alimentos entre sí y escalar a cualquier porción con una regla de
    /// tres. Las porciones concretas del plan se calculan desde acá.
    /// </summary>
    public class Alimento
    {
        public int Id { get; set; }

        /// <summary>Identificador estable y legible (ej. "pechuga-de-pollo"). Único.</summary>
        public string Slug { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre en inglés tal como figura en USDA. Se guarda para poder rastrear
        /// de dónde salió cada número si alguno se ve raro.
        /// </summary>
        public string NombreIngles { get; set; } = string.Empty;

        /// <summary>Grupo culinario: "proteina", "carbohidrato", "verdura", "fruta", "grasa", "lacteo".</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Grupo de intercambio: alimentos que cumplen el mismo papel nutricional en
        /// una comida y por lo tanto pueden sustituirse entre sí. Es la base del
        /// sistema de equivalencias que usa un nutricionista.
        /// </summary>
        public string GrupoIntercambio { get; set; } = string.Empty;

        public double ProteinaPor100g { get; set; }
        public double CarbohidratoPor100g { get; set; }
        public double GrasaPor100g { get; set; }
        public double FibraPor100g { get; set; }

        /// <summary>
        /// Calorías por 100 g derivadas de los macros con los factores de Atwater.
        ///
        /// No es una columna a propósito: si se guardara aparte podría quedar en
        /// desacuerdo con los gramos, y entonces el plan mostraría un total que no
        /// se corresponde con los alimentos que lista. Calculada, eso es imposible.
        /// </summary>
        public double CaloriasPor100g =>
            ProteinaPor100g * ObjetivoMacros.KcalPorGramoProteina
            + CarbohidratoPor100g * ObjetivoMacros.KcalPorGramoCarbohidrato
            + GrasaPor100g * ObjetivoMacros.KcalPorGramoGrasa;

        /// <summary>Porción habitual en gramos, para no proponer cantidades absurdas.</summary>
        public double PorcionTipicaG { get; set; } = 100;

        /// <summary>Cómo se mide esa porción en la cocina (ej. "1 pechuga mediana", "1 taza cocido").</summary>
        public string DescripcionPorcion { get; set; } = string.Empty;

        /// <summary>
        /// Mínimo y máximo razonables al escalar una porción. Sin esto, cuadrar los
        /// macros podría pedir 12 g de arroz o 900 g de brócoli.
        /// </summary>
        public double PorcionMinimaG { get; set; } = 30;
        public double PorcionMaximaG { get; set; } = 300;

        /// <summary>
        /// Etiquetas de dieta que cumple ("vegetariano", "vegano", "sin-gluten",
        /// "sin-lactosa"). Se usan para excluir alimentos según las preferencias.
        /// </summary>
        public List<string> EtiquetasDieta { get; set; } = new();

        /// <summary>
        /// En qué comidas del día cae bien: "desayuno", "principal", "snack".
        ///
        /// Es criterio culinario, no nutricional, y hace falta igual: 115 g de tempeh
        /// con pasta a las siete de la mañana cuadra los macros perfectamente y no lo
        /// desayuna nadie. Un plan que no se sigue no sirve de nada.
        /// </summary>
        public List<string> MomentosAptos { get; set; } = new();

        /// <summary>Imagen de referencia. Puede faltar: la vista degrada a un placeholder.</summary>
        public string? UrlImagen { get; set; }

        /// <summary>
        /// Autor y licencia de la imagen. Se guardan porque las fotos vienen de
        /// Wikimedia Commons bajo CC BY-SA, y atribuir es condición de la licencia,
        /// no un detalle opcional: sin esto no se puede mostrar la foto.
        /// </summary>
        public string? AutorImagen { get; set; }
        public string? LicenciaImagen { get; set; }

        /// <summary>Atribución lista para mostrar al pie de la imagen.</summary>
        public string? AtribucionImagen =>
            UrlImagen is null ? null : $"{AutorImagen} · {LicenciaImagen} (Wikimedia Commons)";

        /// <summary>Id en USDA FoodData Central, para poder auditar el origen del dato.</summary>
        public int? FdcId { get; set; }

        /// <summary>
        /// Escala los macros a una cantidad concreta. Todo el plan pasa por acá, así
        /// que el reparto por porciones no puede desviarse de los datos del catálogo.
        /// </summary>
        public MacrosPorcion MacrosPara(double gramos)
        {
            if (gramos < 0)
                throw new ArgumentOutOfRangeException(nameof(gramos), gramos,
                    "Una porción no puede pesar menos que nada.");

            var factor = gramos / 100.0;

            return new MacrosPorcion(
                Gramos: gramos,
                Calorias: CaloriasPor100g * factor,
                ProteinaG: ProteinaPor100g * factor,
                CarbohidratoG: CarbohidratoPor100g * factor,
                GrasaG: GrasaPor100g * factor);
        }

        /// <summary>
        /// El macro que define al alimento: "proteina", "carbohidrato" o "grasa".
        ///
        /// Para la mayoría lo dice la categoría. Los lácteos y las mezclas se resuelven
        /// por lo que realmente aportan: el yogur griego es proteína, la ricotta es grasa.
        /// Verduras y frutas no cuadran macros, pero su carbohidrato es lo más definitorio.
        ///
        /// Es lo que decide con qué se puede sustituir un alimento y qué se iguala al
        /// hacerlo, así que vive acá y no enterrado en la estrategia que arma el plan.
        /// </summary>
        public string MacroPrincipal => Categoria switch
        {
            "proteina" => "proteina",
            "carbohidrato" or "fruta" or "verdura" => "carbohidrato",
            "grasa" => "grasa",
            _ => ProteinaPor100g * KcalPorGramoProteina >= GrasaPor100g * KcalPorGramoGrasa
                ? "proteina"
                : "grasa"
        };

        /// <summary>Los gramos del macro principal en una porción concreta.</summary>
        public double GramosDelMacroPrincipal(double gramos)
        {
            var macros = MacrosPara(gramos);
            return MacroPrincipal switch
            {
                "proteina" => macros.ProteinaG,
                "carbohidrato" => macros.CarbohidratoG,
                _ => macros.GrasaG
            };
        }

        private const int KcalPorGramoProteina = ObjetivoMacros.KcalPorGramoProteina;
        private const int KcalPorGramoGrasa = ObjetivoMacros.KcalPorGramoGrasa;

        public bool Cumple(string etiqueta) =>
            EtiquetasDieta.Contains(etiqueta, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Si el alimento cae bien en ese momento del día. Sin momentos declarados se
        /// acepta en cualquiera: es preferible a dejarlo fuera del catálogo por omisión.
        /// </summary>
        public bool VaEn(string momento) =>
            MomentosAptos.Count == 0
            || MomentosAptos.Contains(momento, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Macros ya escalados a una porción concreta.</summary>
    public readonly record struct MacrosPorcion(
        double Gramos,
        double Calorias,
        double ProteinaG,
        double CarbohidratoG,
        double GrasaG);
}
