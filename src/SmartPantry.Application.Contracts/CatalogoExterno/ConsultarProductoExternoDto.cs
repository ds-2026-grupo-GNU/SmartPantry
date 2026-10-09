using System.ComponentModel.DataAnnotations;
namespace SmartPantry.CatalogoExterno
{
    public class ConsultarProductoExternoDto
    {
        [Required]
        [RegularExpression(@"^\d{8,14}$", ErrorMessage = "El código de barras debe tener entre 8 y 14 dígitos.")]
        public string CodigoBarras { get; set; } = string.Empty;
    }
}
