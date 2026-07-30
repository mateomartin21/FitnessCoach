using FitnessCoach.Domain.Catalogos;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Entrenamiento;
using Xunit;

namespace FitnessCoach.Tests.Catalogos
{
    /// <summary>
    /// Los índices son lo que se guarda en caché en vez de volver a consultar SQL, así que
    /// tienen que responder lo MISMO que respondía la consulta: mismo orden, y sin
    /// distinguir mayúsculas (la colación de SQL Server tampoco distinguía). Si acá se
    /// colara una diferencia, una rutina se armaría con menos ejercicios sin que falle nada.
    /// </summary>
    public class IndicesDeCatalogoTests
    {
        private static Ejercicio Ejercicio(string slug, string nombre, string grupo) => new()
        {
            Slug = slug, Nombre = nombre, GrupoMuscular = grupo, Equipo = "barbell"
        };

        [Fact]
        public void ElIndiceDeEjercicios_DevuelveTodoOrdenadoPorNombre()
        {
            var indice = IndiceEjercicios.Armar(new[]
            {
                Ejercicio("sentadilla", "Sentadilla", "piernas"),
                Ejercicio("curl", "Curl de biceps", "brazos"),
                Ejercicio("press", "Press de banca", "pecho")
            });

            Assert.Equal(new[] { "Curl de biceps", "Press de banca", "Sentadilla" },
                         indice.Todos.Select(e => e.Nombre));
        }

        [Fact]
        public void ElIndiceDeEjercicios_BuscaPorSlugYPorGrupo()
        {
            var indice = IndiceEjercicios.Armar(new[]
            {
                Ejercicio("sentadilla", "Sentadilla", "piernas"),
                Ejercicio("prensa", "Prensa", "piernas"),
                Ejercicio("press", "Press de banca", "pecho")
            });

            Assert.Equal("Sentadilla", indice.PorSlug("sentadilla")?.Nombre);
            Assert.Equal(2, indice.PorGrupoMuscular("piernas").Count);
            Assert.Equal(new[] { "Prensa", "Sentadilla" },
                         indice.PorGrupoMuscular("piernas").Select(e => e.Nombre));
        }

        [Fact]
        public void ElIndiceDeEjercicios_NoDistingueMayusculas()
        {
            var indice = IndiceEjercicios.Armar(new[] { Ejercicio("sentadilla", "Sentadilla", "Piernas") });

            Assert.NotNull(indice.PorSlug("SENTADILLA"));
            Assert.Single(indice.PorGrupoMuscular("piernas"));
        }

        [Fact]
        public void ElIndiceDeEjercicios_ConClaveDesconocida_DevuelveVacioONulo()
        {
            var indice = IndiceEjercicios.Armar(new[] { Ejercicio("sentadilla", "Sentadilla", "piernas") });

            Assert.Null(indice.PorSlug("no-existe"));
            Assert.Null(indice.PorSlug(null));
            Assert.Empty(indice.PorGrupoMuscular("antebrazo"));
            Assert.Empty(indice.PorGrupoMuscular(null));
        }

        [Fact]
        public void ElIndiceVacio_NoRevienta()
        {
            Assert.Empty(IndiceEjercicios.Armar(Array.Empty<Ejercicio>()).Todos);
            Assert.Empty(IndiceAlimentos.Armar(Array.Empty<Alimento>()).Todos);
        }

        private static Alimento Alimento(string slug, string nombre, string categoria, string grupo) => new()
        {
            Slug = slug, Nombre = nombre, Categoria = categoria, GrupoIntercambio = grupo
        };

        [Fact]
        public void ElIndiceDeAlimentos_BuscaPorSlugCategoriaYGrupoDeIntercambio()
        {
            var indice = IndiceAlimentos.Armar(new[]
            {
                Alimento("pollo", "Pechuga de pollo", "proteina", "proteina-magra"),
                Alimento("atun", "Atún", "proteina", "proteina-magra"),
                Alimento("arroz", "Arroz", "cereal", "carbohidrato")
            });

            Assert.Equal("Atún", indice.PorSlug("atun")?.Nombre);
            Assert.Equal(2, indice.PorCategoria("proteina").Count);
            Assert.Equal(2, indice.PorGrupoIntercambio("proteina-magra").Count);
            Assert.Single(indice.PorCategoria("cereal"));
            Assert.Empty(indice.PorCategoria("postre"));
        }

        [Fact]
        public void ElIndiceDeAlimentos_OrdenaPorNombreYNoDistingueMayusculas()
        {
            var indice = IndiceAlimentos.Armar(new[]
            {
                Alimento("pollo", "Pechuga de pollo", "Proteina", "proteina-magra"),
                Alimento("arroz", "Arroz", "cereal", "carbohidrato")
            });

            Assert.Equal(new[] { "Arroz", "Pechuga de pollo" }, indice.Todos.Select(a => a.Nombre));
            Assert.Single(indice.PorCategoria("proteina"));
            Assert.NotNull(indice.PorSlug("POLLO"));
        }
    }
}
