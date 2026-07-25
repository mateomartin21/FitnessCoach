namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Un proveedor de IA no pudo generar una respuesta. Que sea una excepción y no un
    /// string de error es lo que permite a la cadena distinguir un fallo de una respuesta
    /// buena y pasar al siguiente proveedor (D-09).
    /// </summary>
    public class CoachIAException : Exception
    {
        public CoachIAException(string message) : base(message) { }
        public CoachIAException(string message, Exception innerException) : base(message, innerException) { }
    }
}
