using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Despensa;

public class CreateDespensaItemDto
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Cantidad { get; set; }

    [StringLength(DespensaConsts.MaxUnidadLength)]
    public string? Unidad { get; set; }

    public DateTime? FechaVencimiento { get; set; }
}