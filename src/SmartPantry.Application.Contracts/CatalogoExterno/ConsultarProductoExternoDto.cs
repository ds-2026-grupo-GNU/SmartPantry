using System.ComponentModel.DataAnnotations;
namespace SmartPantry.CatalogoExterno
{
    public class ConsultarProductoExternoDto
    {
        [Required]
        [StringLength(13, MinimumLength = 8)] // longitud típica de un código de barras
        public string CodigoBarras { get; set; }
    }
}
