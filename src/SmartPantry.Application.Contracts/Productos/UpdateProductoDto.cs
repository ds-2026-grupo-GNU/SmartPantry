using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Productos;

public class UpdateProductoDto
{
    // Usamos DataAnnotations para la validación básica de entrada
    [Required]
    [StringLength(128)] 
    public string Nombre { get; set; }
}