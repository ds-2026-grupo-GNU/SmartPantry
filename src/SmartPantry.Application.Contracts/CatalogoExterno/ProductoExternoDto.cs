namespace SmartPantry.CatalogoExterno;

public class ProductoExternoDto
{
    public bool Encontrado { get; set; }

    // Estos campos solo tendrán valor si Encontrado es true y el proveedor los informa
    public string? Nombre { get; set; }
    public string? Marca { get; set; }
    public string? ImagenUrl { get; set; }
}