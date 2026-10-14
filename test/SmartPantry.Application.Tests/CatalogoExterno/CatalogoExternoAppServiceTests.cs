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
        var resultado = await _appService.GetPorCodigoAsync(input);

        // Assert: Comprobamos que el servicio tradujo bien los datos
        resultado.ShouldNotBeNull();
        resultado.Estado.ShouldBe(EstadoConsultaProductoExterno.Encontrado);
        resultado.Nombre.ShouldBe("Nutella");
        resultado.Marca.ShouldBe("Ferrero");
        resultado.ImagenUrl.ShouldBe("http://ejemplo.com/nutella.jpg");
    }

    [Fact]
    public async Task Deberia_Retornar_No_Encontrado_Cuando_El_Producto_No_Existe()
    {
        // Arrange: Simulamos que el mock devuelve null (404 Not Found)
        var codigo = "0000000000000";
        _mockClient.GetByBarcodeAsync(codigo).Returns((ExternalProductDto?)null);

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act
        var resultado = await _appService.GetPorCodigoAsync(input);

        // Assert
        resultado.ShouldNotBeNull();
        resultado.Estado.ShouldBe(EstadoConsultaProductoExterno.NoEncontrado);
        resultado.Nombre.ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Manejar_Datos_Ausentes_Correctamente()
    {
        // Arrange: Simulamos un producto que no tiene marca ni imagen (RF-09)
        var codigo = "12345678";
        _mockClient.GetByBarcodeAsync(codigo).Returns(new ExternalProductDto
        {
            Nombre = "Producto Genérico",
            Marca = null,
            ImagenUrl = null
        });

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act
        var resultado = await _appService.GetPorCodigoAsync(input);

        // Assert
        resultado.ShouldNotBeNull();
        resultado.Estado.ShouldBe(EstadoConsultaProductoExterno.Encontrado);
        resultado.Nombre.ShouldBe("Producto Genérico");
        resultado.Marca.ShouldBeNull(); // No debe inventar datos
        resultado.ImagenUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Retornar_Limite_De_Consultas_Cuando_El_Proveedor_Responde_429()
    {
        // Arrange: Simulamos el error 429 Too Many Requests
        var codigo = "11111111";
        _mockClient.GetByBarcodeAsync(codigo).ThrowsAsync(new ProveedorLimiteConsultasException());

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act: la excepción no debe escapar del servicio
        var resultado = await _appService.GetPorCodigoAsync(input);

        // Assert
        resultado.ShouldNotBeNull();
        resultado.Estado.ShouldBe(EstadoConsultaProductoExterno.LimiteDeConsultas);
        resultado.Nombre.ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Retornar_Proveedor_No_Disponible_Cuando_El_Proveedor_Falla()
    {
        // Arrange: Simulamos un error de red, timeout o HTTP 500
        var codigo = "99999999";
        _mockClient.GetByBarcodeAsync(codigo).ThrowsAsync(new ProveedorNoDisponibleException("El catálogo externo de productos no está disponible."));

        var input = new ConsultarProductoExternoDto { CodigoBarras = codigo };

        // Act: la excepción no debe escapar del servicio
        var resultado = await _appService.GetPorCodigoAsync(input);

        // Assert
        resultado.ShouldNotBeNull();
        resultado.Estado.ShouldBe(EstadoConsultaProductoExterno.ProveedorNoDisponible);
        resultado.Nombre.ShouldBeNull();
    }
}
