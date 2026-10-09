using System;

namespace SmartPantry.CatalogoExterno;

// El proveedor externo respondió 429 (límite de consultas excedido)
public class ProveedorLimiteConsultasException : Exception
{
    public ProveedorLimiteConsultasException()
        : base("El proveedor externo alcanzó el límite de consultas.")
    {
    }
}
