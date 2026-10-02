using System.Threading.Tasks;

namespace SmartPantry.CatalogoExterno;

public interface IExternalProductCatalogClient
{
   
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}
public class ExternalProductDto
{
    public string? Nombre { get; set; }
    public string? Marca { get; set; }
    public string? ImagenUrl { get; set; }
}