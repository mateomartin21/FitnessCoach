namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Crea los proveedores de IA disponibles, en el orden en que la cadena debe
    /// probarlos. Es un Factory: qué proveedores existen y con qué modelos/claves se
    /// decide en un solo lugar a partir de la configuración, no desperdigado por el
    /// cableado. Agregar un proveedor nuevo (otra empresa, otro modelo) es tocar solo
    /// la fábrica.
    ///
    /// Devuelve únicamente los proveedores de IA real (los que hablan con un modelo).
    /// El respaldo offline se agrega aparte como última garantía, porque no depende de
    /// configuración: siempre está.
    /// </summary>
    public interface IFabricaProveedoresIA
    {
        IReadOnlyList<IProveedorIA> CrearProveedores();
    }
}
