using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.CatalogoExterno;

public class CatalogoExternoAppService : ApplicationService, ICatalogoExternoAppService
{
    private readonly IExternalProductCatalogClient _externalCatalogClient;

    // Recibimos la dependencia mediante inyección.
    // ¡El AppService no sabe nada de HTTP ni de Open Food Facts!
    public CatalogoExternoAppService(IExternalProductCatalogClient externalCatalogClient)
    {
        _externalCatalogClient = externalCatalogClient;
    }

    public async Task<ProductoExternoDto> GetPorCodigoAsync(ConsultarProductoExternoDto input)
    {
        ExternalProductDto? productoExterno;

        try
        {
            // 1. Consultar a través de la interfaz puente
            productoExterno = await _externalCatalogClient.GetByBarcodeAsync(input.CodigoBarras);
        }
        catch (ProveedorLimiteConsultasException)
        {
            // El proveedor respondió 429: se informa como estado, nunca como excepción
            return new ProductoExternoDto
            {
                Estado = EstadoConsultaProductoExterno.LimiteDeConsultas
            };
        }
        catch (ProveedorNoDisponibleException)
        {
            // Error HTTP, timeout o respuesta ilegible
            return new ProductoExternoDto
            {
                Estado = EstadoConsultaProductoExterno.ProveedorNoDisponible
            };
        }

        // 2. Traducir el resultado: Si es null, no se encontró (RF-05 / Fallas)
        if (productoExterno == null)
        {
            return new ProductoExternoDto
            {
                Estado = EstadoConsultaProductoExterno.NoEncontrado
            };
        }

        // 3. Traducir el resultado: Se encontró, mapeamos los campos útiles (RF-09 Datos incompletos)
        return new ProductoExternoDto
        {
            Estado = EstadoConsultaProductoExterno.Encontrado,
            Nombre = productoExterno.Nombre,
            Marca = productoExterno.Marca,
            ImagenUrl = productoExterno.ImagenUrl
        };
    }
}
