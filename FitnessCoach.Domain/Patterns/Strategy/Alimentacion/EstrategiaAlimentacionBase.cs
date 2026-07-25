using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    /// <summary>
    /// Compone un plan de comidas tomando alimentos del catálogo y ajustando las
    /// porciones a los macros del usuario.
    ///
    /// Las estrategias concretas dejan de contener comidas escritas a mano: solo
    /// declaran la estructura del día (cuántas comidas, a qué hora, qué papel cumple
    /// cada alimento y qué parte del total aporta la comida).
    ///
    /// El reparto sigue el mismo orden que <c>CalculadorMacros</c>, y no por casualidad:
    /// primero se sirve la proteína, que es el macro con requerimiento absoluto; después
    /// el carbohidrato con lo que quede; la grasa al final. Verduras y frutas van en su
    /// porción habitual porque su papel es el volumen y los micronutrientes, no cuadrar
    /// una cuenta.
    ///
    /// La selección es determinista: el mismo usuario ve siempre su plan. Si fuera
    /// aleatoria, el plan cambiaría al refrescar la página.
    /// </summary>
    public abstract class EstrategiaAlimentacionBase : IEstrategiaAlimentacion
    {
        private readonly IRepositorioAlimentos _catalogo;
        private readonly int _semillaRotacion;

        /// <summary>Las porciones se redondean a esto: nadie pesa 137 g de arroz.</summary>
        private const double MultiploDeRedondeoG = 5;

        protected EstrategiaAlimentacionBase(IRepositorioAlimentos catalogo, int semillaRotacion = 0)
        {
            _catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
            _semillaRotacion = semillaRotacion;
        }

        protected abstract string NombrePlan { get; }
        protected abstract string Objetivo { get; }
        protected abstract string Descripcion { get; }
        protected abstract IReadOnlyList<PlantillaComida> Estructura { get; }
        protected abstract IReadOnlyList<string> Recomendaciones { get; }

        public PlanAlimentacion GenerarPlan(ObjetivoMacros macrosDiarios)
        {
            var plan = new PlanAlimentacion
            {
                NombrePlan = NombrePlan,
                Objetivo = Objetivo,
                Descripcion = Descripcion,
                Objetivos = macrosDiarios,
                RecomendacionesGenerales = Recomendaciones.ToList()
            };

            // Un alimento no se repite en el día: cinco comidas de pollo con arroz
            // cumplen los macros y no las sigue nadie.
            var yaUsados = new HashSet<string>();

            for (var i = 0; i < Estructura.Count; i++)
            {
                var comida = Componer(Estructura[i], macrosDiarios, i, yaUsados);

                // Una comida sin alimentos no se agrega: pasa si el catálogo está
                // vacío, y un plan con huecos confunde más que uno más corto.
                if (comida.Porciones.Count > 0)
                    plan.Comidas.Add(comida);
            }

            return plan;
        }

        private ComidaDia Componer(
            PlantillaComida plantilla,
            ObjetivoMacros diarios,
            int indice,
            HashSet<string> yaUsados)
        {
            var comida = new ComidaDia { NombreComida = plantilla.Nombre, Hora = plantilla.Hora };

            // Lo que esta comida debe aportar.
            double proteinaPendiente = diarios.ProteinaG * plantilla.ParteDelDia;
            double carbohidratoPendiente = diarios.CarbohidratoG * plantilla.ParteDelDia;
            double grasaPendiente = diarios.GrasaG * plantilla.ParteDelDia;

            var elegidos = new List<Alimento>();
            foreach (var rol in plantilla.Roles)
                elegidos.AddRange(Elegir(rol, plantilla.Momento, indice, yaUsados));

            // Primera pasada: verduras y frutas en su porción habitual. Aportan poco
            // pero algo aportan, y hay que descontarlo antes de repartir el resto.
            var deVolumen = elegidos.Where(EsDeVolumen).ToList();
            foreach (var alimento in deVolumen)
            {
                var gramos = Redondear(alimento.PorcionTipicaG);
                var macros = alimento.MacrosPara(gramos);

                proteinaPendiente -= macros.ProteinaG;
                carbohidratoPendiente -= macros.CarbohidratoG;
                grasaPendiente -= macros.GrasaG;

                comida.Porciones.Add(new PorcionAlimento { Alimento = alimento, Gramos = gramos });
            }

            // Segunda pasada: los alimentos que sí se escalan, en orden de prioridad.
            // Cada uno cubre su macro y descuenta de los otros lo que arrastra consigo.
            foreach (var alimento in elegidos.Where(a => !EsDeVolumen(a)).OrderBy(Prioridad))
            {
                var porGramo = MacroQueAporta(alimento) switch
                {
                    "proteina" => (objetivo: proteinaPendiente, densidad: alimento.ProteinaPor100g / 100.0),
                    "carbohidrato" => (objetivo: carbohidratoPendiente, densidad: alimento.CarbohidratoPor100g / 100.0),
                    _ => (objetivo: grasaPendiente, densidad: alimento.GrasaPor100g / 100.0)
                };

                // Densidad cero significa que el alimento no aporta el macro que le toca
                // cubrir: se sirve la porción habitual antes que dividir por cero.
                var gramos = porGramo.densidad <= 0
                    ? alimento.PorcionTipicaG
                    : porGramo.objetivo / porGramo.densidad;

                gramos = Redondear(Math.Clamp(gramos, alimento.PorcionMinimaG, alimento.PorcionMaximaG));

                var macros = alimento.MacrosPara(gramos);
                proteinaPendiente -= macros.ProteinaG;
                carbohidratoPendiente -= macros.CarbohidratoG;
                grasaPendiente -= macros.GrasaG;

                comida.Porciones.Add(new PorcionAlimento { Alimento = alimento, Gramos = gramos });
            }

            CompletarProteina(comida, ref proteinaPendiente, plantilla.Momento, indice, yaUsados);

            PoblarSustitutos(comida, plantilla.Momento);

            return comida;
        }

        /// <summary>
        /// Calcula, para cada porción, sus alternativas del mismo grupo de intercambio.
        /// Los candidatos se acotan al momento del día para no ofrecer avena de cena en
        /// lugar del arroz: comparten grupo "cereal" pero no la hora.
        /// </summary>
        private void PoblarSustitutos(ComidaDia comida, string momento)
        {
            foreach (var porcion in comida.Porciones)
            {
                var candidatos = _catalogo
                    .ObtenerPorGrupoIntercambio(porcion.Alimento.GrupoIntercambio)
                    .Where(a => a.VaEn(momento));

                porcion.Sustitutos = CalculadorEquivalencias.Para(porcion, candidatos);
            }
        }

        /// <summary>
        /// Suma fuentes de proteína hasta cubrir lo que falte de la comida.
        ///
        /// Hace falta porque las porciones tienen tope: para alguien de 120 kg en
        /// déficit, el almuerzo pide unos 79 g de proteína, y eso serían 350 g de pollo
        /// cuando el máximo razonable son 250. Con un solo alimento proteico por comida
        /// el plan se quedaba corto justo con quien más lo necesita.
        ///
        /// Es lo mismo que hace un nutricionista al agregar un segundo alimento al plato
        /// en vez de servir una porción imposible de uno solo.
        /// </summary>
        private void CompletarProteina(
            ComidaDia comida, ref double proteinaPendiente, string momento, int indice, HashSet<string> yaUsados)
        {
            // Por debajo de esto no vale la pena sumar otro alimento al plato.
            const double MinimoQueJustificaOtroAlimento = 8;

            // Tope de refuerzos: una comida con cinco fuentes de proteína deja de ser
            // un plato y pasa a ser una lista.
            const int MaximoRefuerzos = 2;

            for (var refuerzo = 0; refuerzo < MaximoRefuerzos; refuerzo++)
            {
                if (proteinaPendiente < MinimoQueJustificaOtroAlimento) return;

                // Se desplaza el índice para no traer siempre el mismo refuerzo.
                var desplazado = indice + 100 * (refuerzo + 1);
                var candidato = Elegir(new RolAlimento("proteina"), momento, desplazado, yaUsados)
                    .Concat(Elegir(new RolAlimento("lacteo"), momento, desplazado, yaUsados))
                    .FirstOrDefault(a => a.ProteinaPor100g > 0);

                if (candidato is null) return;

                var gramos = Redondear(Math.Clamp(
                    proteinaPendiente / (candidato.ProteinaPor100g / 100.0),
                    candidato.PorcionMinimaG,
                    candidato.PorcionMaximaG));

                proteinaPendiente -= candidato.MacrosPara(gramos).ProteinaG;
                comida.Porciones.Add(new PorcionAlimento { Alimento = candidato, Gramos = gramos });
            }
        }

        /// <summary>Verduras y frutas: acompañan, no cuadran macros.</summary>
        private static bool EsDeVolumen(Alimento alimento) =>
            alimento.Categoria is "verdura" or "fruta";

        /// <summary>
        /// Qué macro le toca cubrir a cada alimento. Lo decide el propio alimento
        /// (<see cref="Alimento.MacroPrincipal"/>), que es también lo que define con
        /// qué se lo puede sustituir.
        /// </summary>
        private static string MacroQueAporta(Alimento alimento) => alimento.MacroPrincipal;

        /// <summary>Orden de servido: proteína, después carbohidrato, la grasa al final.</summary>
        private static int Prioridad(Alimento alimento) => MacroQueAporta(alimento) switch
        {
            "proteina" => 0,
            "carbohidrato" => 1,
            _ => 2
        };

        private static double Redondear(double gramos) =>
            Math.Max(MultiploDeRedondeoG, Math.Round(gramos / MultiploDeRedondeoG) * MultiploDeRedondeoG);

        private IEnumerable<Alimento> Elegir(
            RolAlimento rol, string momento, int indiceComida, HashSet<string> yaUsados)
        {
            var todos = _catalogo.ObtenerPorCategoria(rol.Categoria);

            // Solo lo que cae bien a esa hora. Si el filtro deja el papel sin nada, se
            // vuelve al catálogo entero: un desayuno raro es mejor que un desayuno vacío.
            var aptos = todos.Where(a => a.VaEn(momento)).ToList();
            if (aptos.Count > 0) todos = aptos;

            if (todos.Count == 0) return Array.Empty<Alimento>();

            var candidatos = todos.Where(a => !yaUsados.Contains(a.Slug)).ToList();

            // Si el catálogo se quedó sin alimentos nuevos de este papel, se repite uno
            // antes que dejar el puesto vacío: un plan que se queda corto de proteína es
            // peor que uno que repite el pollo.
            if (candidatos.Count == 0) candidatos = todos.ToList();

            var elegidos = candidatos
                .OrderBy(a => OrdenEstable(a.Slug, indiceComida))
                .ThenBy(a => a.Slug, StringComparer.Ordinal)
                .Take(rol.Cantidad)
                .ToList();

            foreach (var alimento in elegidos)
                yaUsados.Add(alimento.Slug);

            return elegidos;
        }

        /// <summary>
        /// Orden pseudoaleatorio pero reproducible: depende del slug, de la semilla y de
        /// la comida, para que el desayuno y la cena no traigan siempre lo mismo.
        /// No se usa string.GetHashCode() porque .NET lo aleatoriza por proceso, así que
        /// el plan cambiaría en cada reinicio del servidor.
        ///
        /// La semilla se mezcla **después** de recorrer el slug, no antes: sumada al
        /// principio queda multiplicada por 31^longitud, con lo que aporta el mismo
        /// desplazamiento a todos los slugs de igual largo y el orden relativo no cambia.
        /// El paso final de avalancha evita que dos semillas contiguas den órdenes
        /// parecidos, que es justo el caso de dos usuarios con ids consecutivos.
        /// </summary>
        private int OrdenEstable(string slug, int indiceComida)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in slug)
                    hash = hash * 31 + c;

                hash = hash * 31 + _semillaRotacion;
                hash = hash * 31 + indiceComida;

                hash ^= hash >> 15;
                hash *= (int)0x2c1b3c6d;
                hash ^= hash >> 12;

                return hash & 0x7FFFFFFF;
            }
        }
    }
}
