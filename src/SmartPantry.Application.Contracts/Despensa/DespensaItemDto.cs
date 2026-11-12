using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Despensa;

public class DespensaItemDto : EntityDto<Guid>
{
    public Guid ProductoId { get; set; }
    public string? NombreProducto { get; set; }
    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public EstadoItemDespensa Estado { get; set; }
}