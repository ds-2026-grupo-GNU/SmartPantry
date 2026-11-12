using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Despensa;

public class AdvertenciaVencimiento : AuditedAggregateRoot<Guid>
{
    public Guid DespensaItemId { get; set; }
    public bool Activa { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    // Fecha de vencimiento del ítem (parte fecha, UTC) para la que se calculó la advertencia
    public DateTime FechaVencimiento { get; set; }
    public DateTime FechaCalculo { get; set; }
}
