using System.Threading.Tasks;
using SmartPantry.EntityFrameworkCore; 
using SmartPantry.Productos;
using Shouldly;
using Xunit;

namespace SmartPantry.Productos;

public class ProductoAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly IProductoAppService _productoAppService;

    public ProductoAppService_Tests()
    {
        _productoAppService = GetRequiredService<IProductoAppService>();
    }

    [Fact]
    public async Task Deberia_Crear_Producto_Correctamente()
    {
        // Arrange
        var input = new CreateProductoDto { Nombre = "Harina" };

        // Act
        var productoCreado = await _productoAppService.CreateAsync(input);

        // Assert
        productoCreado.ShouldNotBeNull();
        productoCreado.Nombre.ShouldBe("Harina");
    }
}