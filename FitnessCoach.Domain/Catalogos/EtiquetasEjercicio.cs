namespace FitnessCoach.Domain.Catalogos
{
    /// <summary>
    /// El catálogo viene en inglés y con vocabulario de base de datos ("pectorals",
    /// "ez-bar"). Acá se traduce a lo que la app enseña, que habla español.
    ///
    /// Lo que no esté en la tabla se devuelve tal cual: es preferible mostrar el valor
    /// crudo que un hueco, y así un grupo nuevo del catálogo no rompe ninguna pantalla.
    /// </summary>
    public static class EtiquetasEjercicio
    {
        private static readonly Dictionary<string, string> GruposMusculares = new(StringComparer.OrdinalIgnoreCase)
        {
            ["abductors"] = "abductores",
            ["abs"] = "abdomen",
            ["adductors"] = "aductores",
            ["biceps"] = "bíceps",
            ["calves"] = "pantorrillas",
            ["cardio"] = "cardio",
            ["delts"] = "hombros",
            ["forearms"] = "antebrazos",
            ["glutes"] = "glúteos",
            ["hamstrings"] = "isquiotibiales",
            ["lats"] = "dorsales",
            ["levator-scapulae"] = "cuello",
            ["pectorals"] = "pecho",
            ["quads"] = "cuádriceps",
            ["serratus-anterior"] = "serrato",
            ["spine"] = "espalda baja",
            ["traps"] = "trapecios",
            ["triceps"] = "tríceps",
            ["upper-back"] = "espalda alta"
        };

        private static readonly Dictionary<string, string> Equipos = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bodyweight"] = "peso corporal",
            ["dumbbell"] = "mancuerna",
            ["barbell"] = "barra",
            ["ez-bar"] = "barra Z",
            ["cable"] = "polea",
            ["band"] = "banda elástica",
            ["kettlebell"] = "pesa rusa",
            ["lever"] = "máquina",
            ["smith"] = "multipower",
            ["machine"] = "máquina",
            ["sled"] = "trineo",
            ["other"] = "otro"
        };

        public static string GrupoMuscular(string? valor) => Traducir(GruposMusculares, valor);

        public static string Equipo(string? valor) => Traducir(Equipos, valor);

        // Para comprobar la cobertura hace falta preguntar por la clave y no comparar el
        // resultado: "cardio" se escribe igual en los dos idiomas y pareceria sin traducir.
        public static bool ConoceGrupoMuscular(string? valor) =>
            !string.IsNullOrWhiteSpace(valor) && GruposMusculares.ContainsKey(valor);

        public static bool ConoceEquipo(string? valor) =>
            !string.IsNullOrWhiteSpace(valor) && Equipos.ContainsKey(valor);

        private static string Traducir(Dictionary<string, string> tabla, string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;

            return tabla.TryGetValue(valor, out var etiqueta) ? etiqueta : valor;
        }
    }
}
