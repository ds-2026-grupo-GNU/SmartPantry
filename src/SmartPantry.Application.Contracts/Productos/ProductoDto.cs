using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Productos;

public class ProductoDto : EntityDto<Guid>
{
    public string Nombre { get; set; }
}