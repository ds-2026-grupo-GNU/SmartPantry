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

    [Fact]
    public async Task Deberia_Realizar_CRUD_Completo_De_Producto()
    {
        // 1. REGISTRAR (Create)
        var createDto = new CreateProductoDto { Nombre = "Azúcar" };
        var productoCreado = await _productoAppService.CreateAsync(createDto);

        productoCreado.ShouldNotBeNull();
        productoCreado.Nombre.ShouldBe("Azúcar");

        // 2. LISTAR (GetList - Paginado)
        // Usamos el DTO de paginación nativo de ABP
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

        // 6. COMPROBAR ELIMINACIÓN (Demostrar qué sucede al consultar luego de borrar)
        await Assert.ThrowsAsync<EntityNotFoundException<Producto>>(async () =>
        {
            await _productoAppService.GetAsync(productoCreado.Id);
        });

    }
}