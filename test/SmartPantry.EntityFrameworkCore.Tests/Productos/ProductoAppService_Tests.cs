using System;
using System.Threading.Tasks;
using SmartPantry.EntityFrameworkCore;
using SmartPantry.Productos;
using Shouldly;
using Xunit;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Productos;

public class ProductoAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly IProductoAppService _productoAppService;

    public ProductoAppService_Tests()
    {
        _productoAppService = GetRequiredService<IProductoAppService>();
    }

    // TP05
    [Fact]
    public async Task Deberia_Crear_Producto_Correctamente()
    {
        var input = new CreateProductoDto { Nombre = "Harina" };

        var productoCreado = await _productoAppService.CreateAsync(input);

        productoCreado.ShouldNotBeNull();
        productoCreado.Nombre.ShouldBe("Harina");
    }

    // TP05 (corrección pedida por la cátedra)
    [Fact]
    public async Task Deberia_Obtener_Producto_Por_Id_Correctamente()
    {
        var input = new CreateProductoDto { Nombre = "Fideos" };
        var productoCreado = await _productoAppService.CreateAsync(input);

        var productoObtenido = await _productoAppService.GetAsync(productoCreado.Id);

        productoObtenido.ShouldNotBeNull();
        productoObtenido.Id.ShouldBe(productoCreado.Id);
        productoObtenido.Nombre.ShouldBe("Fideos");
    }

    // TP06
    [Fact]
    public async Task Deberia_Realizar_CRUD_Completo_De_Producto()
    {
        // 1. REGISTRAR (Create)
        var createDto = new CreateProductoDto { Nombre = "Azúcar" };
        var productoCreado = await _productoAppService.CreateAsync(createDto);

        productoCreado.ShouldNotBeNull();
        productoCreado.Nombre.ShouldBe("Azúcar");

        // 2. LISTAR (GetList - Paginado)
        var listResult = await _productoAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        listResult.TotalCount.ShouldBeGreaterThan(0);
        listResult.Items.ShouldContain(p => p.Id == productoCreado.Id);

        // 3. MODIFICAR (Update)
        var updateDto = new UpdateProductoDto { Nombre = "Azúcar Mascabo" };
        var productoActualizado = await _productoAppService.UpdateAsync(productoCreado.Id, updateDto);

        productoActualizado.Nombre.ShouldBe("Azúcar Mascabo");

        // 4. CONSULTAR (Get)
        var productoConsultado = await _productoAppService.GetAsync(productoCreado.Id);
        productoConsultado.Nombre.ShouldBe("Azúcar Mascabo");

        // 5. ELIMINAR (Delete)
        await _productoAppService.DeleteAsync(productoCreado.Id);

        // 6. COMPROBAR ELIMINACIÓN
        await Assert.ThrowsAsync<EntityNotFoundException<Producto>>(async () =>
        {
            await _productoAppService.GetAsync(productoCreado.Id);
        });
    }
}