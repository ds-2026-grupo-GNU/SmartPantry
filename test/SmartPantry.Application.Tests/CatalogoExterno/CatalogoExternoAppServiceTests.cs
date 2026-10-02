using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;
using SmartPantry.CatalogoExterno;

namespace SmartPantry.Application.Tests.CatalogoExterno;

public class CatalogoExternoAppServiceTests
{
    private readonly IExternalProductCatalogClient _mockClient;
    private readonly CatalogoExternoAppService _appService;

    public CatalogoExternoAppServiceTests()
    {
        // 1. Creamos un "doble" falso de la interfaz usando NSubstitute
        _mockClient = Substitute.For<IExternalProductCatalogClient>();

        // 2. Le inyectamos este doble al servicio real
        _appService = new CatalogoExternoAppService(_mockClient);
    }

    [Fact]
    public async Task Deberia_Retornar_Encontrado_Cuando_El_Producto_Existe()
    {
       
        var codigo = "3017620422003";
        _mockClient.GetByBarcodeAsync(codigo).Returns(new ExternalProductDto
        {
            Nombre = "Nutella",
            Marca = "Ferrero",
            ImagenUrl = "http://ejemplo.com/nutella.jpg"
        });

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act:Ejecutamos el servicio
        var resultado = await _appService.ConsultarPorCodigoAsync(input);

        // Assert: Comprobamos que el servicio tradujo bien los datos
        resultado.ShouldNotBeNull();
        resultado.Encontrado.ShouldBeTrue();
        resultado.Nombre.ShouldBe("Nutella");
        resultado.Marca.ShouldBe("Ferrero");
    }

    [Fact]
    public async Task Deberia_Retornar_No_Encontrado_Cuando_El_Producto_No_Existe()
    {
        // Arrange: Simulamos que el mock devuelve null (404 Not Found)
        var codigo = "0000000000000";
        _mockClient.GetByBarcodeAsync(codigo).Returns((ExternalProductDto?)null);

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act
        var resultado = await _appService.ConsultarPorCodigoAsync(input);

        // Assert
        resultado.ShouldNotBeNull();
        resultado.Encontrado.ShouldBeFalse();
        resultado.Nombre.ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Manejar_Datos_Ausentes_Correctamente()
    {
        // Arrange: Simulamos un producto que no tiene marca ni imagen (RF-09)
        var codigo = "123456789";
        _mockClient.GetByBarcodeAsync(codigo).Returns(new ExternalProductDto
        {
            Nombre = "Producto Genérico",
            Marca = null,
            ImagenUrl = null
        });

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act
        var resultado = await _appService.ConsultarPorCodigoAsync(input);

        // Assert
        resultado.Encontrado.ShouldBeTrue();
        resultado.Nombre.ShouldBe("Producto Genérico");
        resultado.Marca.ShouldBeNull(); // No debe inventar datos
    }

    [Fact]
    public async Task Deberia_Propagar_Excepcion_Cuando_Hay_Limite_De_Consultas()
    {
        // Arrange: Simulamos el error 429 Too Many Requests
        var codigo = "111111111";
        _mockClient.GetByBarcodeAsync(codigo).Throws(new ApplicationException("Rate limit exceeded"));

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act & Assert: Verificamos que el servicio deja pasar la excepción
        await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await _appService.ConsultarPorCodigoAsync(input);
        });
    }

    [Fact]
    public async Task Deberia_Propagar_Excepcion_Cuando_Proveedor_No_Disponible()
    {
        // Arrange: Simulamos un error de red o timeout (500)
        var codigo = "999999999";
        _mockClient.GetByBarcodeAsync(codigo).Throws(new ApplicationException("External product catalog is currently unavailable."));

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await _appService.ConsultarPorCodigoAsync(input);
        });
    }
}