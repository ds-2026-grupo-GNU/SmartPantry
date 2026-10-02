using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System.Threading.Tasks;
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

    [Fact]
    public async Task Deberia_Obtener_Producto_Por_Id_Correctamente()
    {
        // Arrange: creamos un producto para tener un Id real
        var input = new CreateProductoDto { Nombre = "Fideos" };
        var productoCreado = await _productoAppService.CreateAsync(input);

        // Act
        var productoObtenido = await _productoAppService.GetAsync(productoCreado.Id);

        // Assert
        productoObtenido.ShouldNotBeNull();
        productoObtenido.Id.ShouldBe(productoCreado.Id);
        productoObtenido.Nombre.ShouldBe("Fideos");
    }
}