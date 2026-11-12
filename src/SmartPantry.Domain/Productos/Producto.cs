using System;
using Volo.Abp; // <-- Necesario para la clase Check
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Productos;

public class Producto : FullAuditedAggregateRoot<Guid>
{
    public string Nombre { get; set; }

    // Constructor vacío para Mapperly y EF Core
    public Producto()
    {
    }

    // Constructor para tus tests y lógica de dominio
    public Producto(Guid id, string nombre) : base(id)
    {
        SetNombre(nombre);
    }

    public void SetNombre(string nombre)
    {
        var nombreNormalizado = nombre?.Trim();

        // Esta línea es la que evalúa el texto y lanza el ArgumentException si está vacío
        Nombre = Check.NotNullOrWhiteSpace(nombreNormalizado, nameof(nombre));
    }
}