namespace FitnessCoach.Domain.Models.Gamificacion
{
    /// <summary>
    /// El catálogo de logros de la app. Están anclados a hechos reales (entrenamientos,
    /// rachas, récords, constancia) y no a métricas vacías: se premia lo que de verdad
    /// mueve la aguja (05-VISION-PRODUCTO §77). Agregar un logro es sumar una línea acá.
    /// </summary>
    public static class CatalogoLogros
    {
        public static readonly IReadOnlyList<Logro> Todos = new[]
        {
            new Logro("primer-entreno", "Primer aullido",
                "Completa tu primer entrenamiento.", "🐺", 25,
                "¡Arrancaste, campeón! El primero es el que cuesta, y ya lo tienes.",
                1, e => e.TotalEntrenamientos),

            new Logro("diez-entrenos", "En marcha",
                "Completa 10 entrenamientos.", "💪", 60,
                "Diez sesiones. Esto ya no es un arranque, es un hábito formándose.",
                10, e => e.TotalEntrenamientos),

            new Logro("cincuenta-entrenos", "Máquina",
                "Completa 50 entrenamientos.", "🏋️", 200,
                "Cincuenta. Poca gente llega aquí; tú sí. Orgulloso de ti, campeón.",
                50, e => e.TotalEntrenamientos),

            new Logro("racha-3", "Tres al hilo",
                "Entrena 3 días seguidos.", "🔥", 40,
                "Tres días pegados. La constancia se ve, no se cuenta.",
                3, e => e.RachaMaxima),

            new Logro("racha-7", "Semana perfecta",
                "Entrena 7 días seguidos.", "⚡", 100,
                "Una semana entera sin fallar. Así se construye un lobo.",
                7, e => e.RachaMaxima),

            new Logro("racha-30", "Imparable",
                "Entrena 30 días seguidos.", "👑", 400,
                "Treinta días. Eres otra persona a la que empezó. Imparable.",
                30, e => e.RachaMaxima),

            new Logro("primer-record", "Nueva marca",
                "Registra tu primer récord personal.", "🎯", 30,
                "Primera marca en la tabla. Ahora tienes a quién ganarle: a ti mismo.",
                1, e => e.TotalRecords),

            new Logro("diez-records", "Rompiendo límites",
                "Registra 10 récords personales.", "📈", 120,
                "Diez marcas. La sobrecarga progresiva en acción, no en la teoría.",
                10, e => e.TotalRecords),

            new Logro("primer-peso", "Subido a la balanza",
                "Registra tu peso por primera vez.", "⚖️", 15,
                "Lo que se mide, se mejora. Primer registro hecho.",
                1, e => e.TotalRegistrosPeso),

            new Logro("diario-7", "Cuentas claras",
                "Registra tu comida en 7 días distintos.", "🍽️", 80,
                "Siete días anotando lo que comes. Ahí está el control de verdad.",
                7, e => e.DiasConDiario),

            new Logro("con-objetivo", "Con rumbo",
                "Define tu objetivo fitness.", "🧭", 20,
                "Con un objetivo claro, cada entrenamiento apunta a algo. Bien ahí.",
                1, e => e.TieneObjetivo ? 1 : 0),

            new Logro("semana-de-fuego", "Semana de fuego",
                "Entrena 3 veces en la última semana.", "🌟", 50,
                "Tres entrenos en la semana. Ese es el ritmo, campeón; no lo sueltes.",
                3, e => e.EntrenamientosEstaSemana),
        };
    }
}
