namespace FitnessCoach.Domain.Models
{
    /// <summary>
    /// Los rangos válidos de un perfil, en un solo lugar.
    /// Los consumen tanto las anotaciones de validación (capa de entrada) como las
    /// guardas del cálculo calórico (capa de dominio), para que no puedan divergir.
    /// Criterio en docs/contexto/03-ESTANDARES.md §1.2.
    /// </summary>
    public static class RangosPerfil
    {
        public const int EdadMinima = 13;
        public const int EdadMaxima = 100;

        public const double PesoMinimoKg = 30;
        public const double PesoMaximoKg = 300;

        public const double EstaturaMinimaCm = 100;
        public const double EstaturaMaximaCm = 250;

        public const int NombreLargoMinimo = 2;
        public const int NombreLargoMaximo = 100;

        public const int NotasLargoMaximo = 500;

        public const int NombreRutinaLargoMaximo = 100;

        // Un entrenamiento de menos de 5 minutos o de más de 5 horas es error de captura.
        public const int DuracionMinimaMin = 5;
        public const int DuracionMaximaMin = 300;
    }
}
