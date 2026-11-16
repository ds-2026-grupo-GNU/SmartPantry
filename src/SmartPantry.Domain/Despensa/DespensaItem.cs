using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Despensa;

public class DespensaItem : AuditedAggregateRoot<Guid>
{
    public Guid PropietarioId { get; set; }
    public Guid ProductoId { get; set; }
    public EstadoItemDespensa Estado { get; set; } // Consumido, Disponible...
    public DateTime? FechaVencimiento { get; set; }

    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }

    public DespensaItem()
    {
    }

    public DespensaItem(
        Guid id,
        Guid propietarioId,
        Guid productoId,
        decimal? cantidad,
        string? unidad,
        DateTime? fechaVencimiento) : base(id)
    {
        PropietarioId = propietarioId;
        ProductoId = productoId;
        Estado = EstadoItemDespensa.Disponible;
        ActualizarDatos(cantidad, unidad, fechaVencimiento);
    }

    public void ActualizarDatos(decimal? cantidad, string? unidad, DateTime? fechaVencimiento)
    {
        if (cantidad < 0)
        {
            throw new ArgumentException("La cantidad no puede ser negativa.", nameof(cantidad));
        }

        var unidadNormalizada = string.IsNullOrWhiteSpace(unidad) ? null : unidad.Trim();
        if (unidadNormalizada != null)
        {
            Check.Length(unidadNormalizada, nameof(unidad), DespensaConsts.MaxUnidadLength);
        }

        Cantidad = cantidad;
        Unidad = unidadNormalizada;
        FechaVencimiento = fechaVencimiento;
    }

    // El worker de desactiva su advertencia en la próxima corrida.
    public void MarcarConsumido()
    {
        Estado = EstadoItemDespensa.Consumido;
    }
}