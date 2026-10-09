using System;
using System.Net;
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
    private const string Campos = "code,product_name,product_name_es,brands,image_front_url";

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
        // Open Food Facts API v3 route (relativa a la BaseAddress configurada en el módulo)
        var requestUri = $"product/{Uri.EscapeDataString(barcode)}?fields={Uri.EscapeDataString(Campos)}";

        try
        {
            using var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // El producto no existe en la base de datos externa
                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Límite de solicitudes excedido (429)
                _logger.LogWarning("Open Food Facts rate limit exceeded for barcode: {Barcode}", barcode);
                throw new ProveedorLimiteConsultasException();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Open Food Facts returned status {StatusCode} for barcode: {Barcode}", (int)response.StatusCode, barcode);
                throw new ProveedorNoDisponibleException(
                    $"El proveedor externo respondió con el estado HTTP {(int)response.StatusCode}.");
            }

            // Deserializamos el JSON de respuesta a nuestra clase interna
            var offResponse = await response.Content.ReadFromJsonAsync<OffProductResponse>();

            if (offResponse?.Product == null)
            {
                return null;
            }

            // Mapeamos a nuestro DTO interno; los campos no informados quedan en null
            return new ExternalProductDto
            {
                Nombre = NormalizarTexto(offResponse.Product.ProductNameEs) ?? NormalizarTexto(offResponse.Product.ProductName),
                Marca = NormalizarTexto(offResponse.Product.Brands),
                ImagenUrl = NormalizarTexto(offResponse.Product.ImageFrontUrl)
            };
        }
        catch (JsonException ex)
        {
            // El proveedor devolvió un JSON ilegible
            _logger.LogError(ex, "Invalid response from Open Food Facts API for barcode: {Barcode}", barcode);
            throw new ProveedorNoDisponibleException("La respuesta del proveedor externo no pudo interpretarse.", ex);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Error de red o del servidor externo (ej. sin conexión, timeout)
            _logger.LogError(ex, "Error communicating with Open Food Facts API for barcode: {Barcode}", barcode);
            throw new ProveedorNoDisponibleException("El catálogo externo de productos no está disponible.", ex);
        }
    }

    // Los textos en blanco que informa el proveedor se tratan como "no informado" (null)
    private static string? NormalizarTexto(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
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

        [JsonPropertyName("product_name_es")]
        public string? ProductNameEs { get; set; }

        [JsonPropertyName("brands")]
        public string? Brands { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? ImageFrontUrl { get; set; }
    }
}
