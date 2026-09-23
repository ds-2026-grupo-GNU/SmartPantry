using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Productos;

public class CreateProductoDto
{
    [Required]
    [StringLength(ProductoConsts.MaxNombreLength)]
    public string Nombre { get; set; }
}