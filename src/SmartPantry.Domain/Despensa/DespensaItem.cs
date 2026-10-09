using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Despensa;

// Heredamos de AuditedAggregateRoot para tener fecha de creación automáticamente
public class DespensaItem : AuditedAggregateRoot<Guid>
{
    public Guid PropietarioId { get; set; } 
    public Guid ProductoId { get; set; } 
    public EstadoItemDespensa Estado { get; set; } // Consumido, Disponible...[cite: 10]
    public DateTime? FechaVencimiento { get; set; }
}