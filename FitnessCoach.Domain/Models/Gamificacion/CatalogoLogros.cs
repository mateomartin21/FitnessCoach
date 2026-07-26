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
                "Completá tu primer entrenamiento.", "🐺", 25,
                "¡Arrancaste, campeon! El primero es el que cuesta, y ya lo tenes.",
                1, e => e.TotalEntrenamientos),

            new Logro("diez-entrenos", "En marcha",
                "Completá 10 entrenamientos.", "💪", 60,
                "Diez sesiones. Esto ya no es un arranque, es un habito formandose.",
                10, e => e.TotalEntrenamientos),

            new Logro("cincuenta-entrenos", "Máquina",
                "Completá 50 entrenamientos.", "🏋️", 200,
                "Cincuenta. Poca gente llega aca; vos si. Orgulloso de vos, campeon.",
                50, e => e.TotalEntrenamientos),

            new Logro("racha-3", "Tres al hilo",
                "Entrená 3 días seguidos.", "🔥", 40,
                "Tres dias pegados. La constancia se ve, no se cuenta.",
                3, e => e.RachaMaxima),

            new Logro("racha-7", "Semana perfecta",
                "Entrená 7 días seguidos.", "⚡", 100,
                "Una semana entera sin fallar. Asi se construye un lobo.",
                7, e => e.RachaMaxima),

            new Logro("racha-30", "Imparable",
                "Entrená 30 días seguidos.", "👑", 400,
                "Treinta dias. Sos otra persona a la que empezo. Imparable.",
                30, e => e.RachaMaxima),

            new Logro("primer-record", "Nueva marca",
                "Registrá tu primer récord personal.", "🎯", 30,
                "Primera marca en la tabla. Ahora tenes a quien ganarle: a vos mismo.",
                1, e => e.TotalRecords),

            new Logro("diez-records", "Rompiendo límites",
                "Registrá 10 récords personales.", "📈", 120,
                "Diez marcas. La sobrecarga progresiva en accion, no en la teoria.",
                10, e => e.TotalRecords),

            new Logro("primer-peso", "Subido a la balanza",
                "Registrá tu peso por primera vez.", "⚖️", 15,
                "Lo que se mide, se mejora. Primer registro hecho.",
                1, e => e.TotalRegistrosPeso),

            new Logro("diario-7", "Cuentas claras",
                "Registrá tu comida en 7 días distintos.", "🍽️", 80,
                "Siete dias anotando lo que comes. Ahi esta el control de verdad.",
                7, e => e.DiasConDiario),

            new Logro("con-objetivo", "Con rumbo",
                "Definí tu objetivo fitness.", "🧭", 20,
                "Con un objetivo claro, cada entrenamiento apunta a algo. Bien ahi.",
                1, e => e.TieneObjetivo ? 1 : 0),

            new Logro("semana-de-fuego", "Semana de fuego",
                "Entrená 3 veces en la última semana.", "🌟", 50,
                "Tres entrenos en la semana. Ese es el ritmo, campeon; no lo sueltes.",
                3, e => e.EntrenamientosEstaSemana),
        };
    }
}
