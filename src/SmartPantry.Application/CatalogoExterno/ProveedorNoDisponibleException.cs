using System;

namespace SmartPantry.CatalogoExterno;

// Error HTTP, timeout, falta de conexión o respuesta ilegible del proveedor externo
public class ProveedorNoDisponibleException : Exception
{
    public ProveedorNoDisponibleException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
