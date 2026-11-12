using System.Threading.Tasks;

namespace SmartPantry.CatalogoExterno;

public interface IExternalProductCatalogClient
{

    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}
