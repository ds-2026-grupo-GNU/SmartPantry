namespace SmartPantry.CatalogoExterno;

public class ProductoExternoDto
{
    public EstadoConsultaProductoExterno Estado { get; set; }

    // Estos campos solo tendrán valor si Estado es Encontrado y el proveedor los informa
    public string? Nombre { get; set; }
    public string? Marca { get; set; }
    public string? ImagenUrl { get; set; }
}
