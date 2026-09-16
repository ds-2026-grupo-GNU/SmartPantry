using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Productos;

public class Producto : AggregateRoot<Guid>
{
    public string Nombre { get; private set; }

    // Constructor privado para Entity Framework Core
    private Producto() { }

    public Producto(Guid id, string nombre) : base(id)
    {
        SetNombre(nombre);
    }

    public void SetNombre(string nombre)
    {
        Nombre = Check.NotNullOrWhiteSpace(
            nombre,
            nameof(nombre),
            maxLength: ProductoConsts.MaxNombreLength
        );
    }
}