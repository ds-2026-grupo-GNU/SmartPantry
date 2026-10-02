using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartPantry.CatalogoExterno;

namespace SmartPantry.HttpApi.Host.CatalogoExterno;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenFoodFactsProductCatalogClient> _logger;

    // Recibimos el HttpClient inyectado por IHttpClientFactory
    public OpenFoodFactsProductCatalogClient(
        HttpClient httpClient,
        ILogger<OpenFoodFactsProductCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        try
        {
            // Open Food Facts API v3 route
            var requestUri = $"/api/v3/product/{barcode}.json?fields=product_name,brands,image_url";

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // El producto no existe en la base de datos externa
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Límite de solicitudes excedido (429)
                _logger.LogWarning("Open Food Facts rate limit exceeded for barcode: {Barcode}", barcode);
                throw new ApplicationException("Rate limit exceeded from external provider.");
            }

            // Aseguramos que la respuesta fue exitosa (200-299)
            response.EnsureSuccessStatusCode();

            // Deserializamos el JSON de respuesta a nuestra clase interna
            var offResponse = await response.Content.ReadFromJsonAsync<OffProductResponse>();

            if (offResponse?.Product == null)
            {
                return null;
            }

            // Mapeamos a nuestro DTO interno
            return new ExternalProductDto
            {
                Nombre = offResponse.Product.ProductName,
                Marca = offResponse.Product.Brands,
                ImagenUrl = offResponse.Product.ImageUrl
            };
        }
        catch (HttpRequestException ex)
        {
            // Error de red o del servidor externo (ej. 500, timeout)
            _logger.LogError(ex, "Error communicating with Open Food Facts API for barcode: {Barcode}", barcode);
            throw new ApplicationException("External product catalog is currently unavailable.", ex);
        }
    }

    // Clases internas para deserializar *solamente* los campos que nos interesan del JSON gigante del proveedor.
    // Esto evita acoplar nuestro sistema al contrato completo de Open Food Facts.
    private class OffProductResponse
    {
        [JsonPropertyName("product")]
        public OffProductData? Product { get; set; }
    }

    private class OffProductData
    {
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("brands")]
        public string? Brands { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }
}