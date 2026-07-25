using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Maneja el diario de comidas: registrar lo comido, borrarlo y resumir el día
    /// contra el objetivo de macros. Une el catálogo (para los macros del alimento)
    /// con el perfil (para el objetivo y la persistencia).
    /// </summary>
    public class ServicioDiario : IServicioDiario
    {
        private readonly IRepositorioUsuario _usuarios;
        private readonly IRepositorioAlimentos _catalogo;
        private readonly ICalculadorCalorico _calculadorCalorico;

        public ServicioDiario(
            IRepositorioUsuario usuarios,
            IRepositorioAlimentos catalogo,
            ICalculadorCalorico calculadorCalorico)
        {
            _usuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
            _catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
            _calculadorCalorico = calculadorCalorico ?? throw new ArgumentNullException(nameof(calculadorCalorico));
        }

        public void Registrar(UsuarioPerfil usuario, string alimentoSlug, double gramos, DateOnly dia)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            if (gramos <= 0)
                throw new ArgumentOutOfRangeException(nameof(gramos), gramos,
                    "No se puede registrar una cantidad nula o negativa.");

            var alimento = _catalogo.ObtenerPorSlug(alimentoSlug)
                ?? throw new ArgumentException($"No existe el alimento '{alimentoSlug}' en el catálogo.", nameof(alimentoSlug));

            // La fecha se guarda en UTC a medianoche del día elegido: el diario es por día,
            // no por hora, y así ordena y agrupa igual que el resto de las fechas del perfil.
            var fecha = DateTime.SpecifyKind(dia.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

            usuario.Diario.Add(RegistroComida.De(alimento, gramos, fecha));
            _usuarios.Guardar(usuario);
        }

        public void Borrar(UsuarioPerfil usuario, int registroId)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var registro = usuario.Diario.FirstOrDefault(r => r.Id == registroId);
            if (registro is null) return;   // no es suyo o ya no está: nada que hacer

            usuario.Diario.Remove(registro);
            _usuarios.Guardar(usuario);
        }

        public ResumenDiario ResumenDelDia(UsuarioPerfil usuario, DateOnly dia)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var registros = usuario.Diario
                .Where(r => DateOnly.FromDateTime(r.Fecha) == dia)
                .OrderBy(r => r.Id)
                .ToList();

            var objetivo = CalcularObjetivo(usuario);

            return new ResumenDiario(dia, objetivo, registros);
        }

        /// <summary>
        /// El objetivo de macros del usuario. Si el perfil todavía no tiene datos válidos
        /// para calcular calorías, se devuelve un objetivo en cero: el diario igual sirve
        /// para anotar, solo que sin una meta contra la cual comparar.
        /// </summary>
        private ObjetivoMacros CalcularObjetivo(UsuarioPerfil usuario)
        {
            try
            {
                var calorias = _calculadorCalorico.CalcularCaloriasDiarias(usuario);
                return CalculadorMacros.Calcular(usuario, calorias);
            }
            catch (ArgumentOutOfRangeException)
            {
                return default;
            }
        }
    }
}
