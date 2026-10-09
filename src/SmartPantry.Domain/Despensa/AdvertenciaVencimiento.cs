using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Despensa;

public class AdvertenciaVencimiento : AuditedAggregateRoot<Guid>
{
    public Guid DespensaItemId { get; set; }
    public bool Activa { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaCalculo { get; set; }
}