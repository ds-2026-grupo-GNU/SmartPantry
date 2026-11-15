using Shouldly;
using SmartPantry.EntityFrameworkCore;
using SmartPantry.Productos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Despensa;

public class DespensaItemAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly IDespensaItemAppService _appService;
    private readonly IRepository<DespensaItem, Guid> _itemRepository;
    private readonly IRepository<Producto, Guid> _productoRepository;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciaRepository;
    private readonly Guid _usuarioActualId;

    public DespensaItemAppService_Tests()
    {
        _appService = GetRequiredService<IDespensaItemAppService>();
        _itemRepository = GetRequiredService<IRepository<DespensaItem, Guid>>();
        _productoRepository = GetRequiredService<IRepository<Producto, Guid>>();
        _advertenciaRepository = GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>();

        // Usuario fijo que define FakeCurrentPrincipalAccessor para toda la suite
        _usuarioActualId = GetRequiredService<ICurrentUser>().GetId();
    }

    [Fact]
    public async Task Deberia_Listar_Solo_Los_Items_Propios_Con_El_Nombre_Del_Producto()
    {
        var productoId = await CrearProductoAsync("Leche");
        var propio = await CrearItemAsync(_usuarioActualId, productoId);
        var ajeno = await CrearItemAsync(Guid.NewGuid(), productoId);

        var resultado = await _appService.GetListAsync(new PagedResultRequestDto());

        resultado.Items.ShouldContain(x => x.Id == propio);
        resultado.Items.ShouldNotContain(x => x.Id == ajeno);
        resultado.Items.Single(x => x.Id == propio).NombreProducto.ShouldBe("Leche");
    }

    [Fact]
    public async Task Deberia_Paginar_El_Listado()
    {
        var productoId = await CrearProductoAsync("Arroz");
        await CrearItemAsync(_usuarioActualId, productoId);
        await CrearItemAsync(_usuarioActualId, productoId);

        var resultado = await _appService.GetListAsync(new PagedResultRequestDto { MaxResultCount = 1 });

        resultado.Items.Count.ShouldBe(1);
        resultado.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Deberia_Devolver_Nombre_Nulo_Si_El_Producto_No_Existe()
    {
        var item = await CrearItemAsync(_usuarioActualId, Guid.NewGuid());

        var resultado = await _appService.GetListAsync(new PagedResultRequestDto());

        resultado.Items.Single(x => x.Id == item).NombreProducto.ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Crear_Item_Con_El_Usuario_Actual_Como_Propietario()
    {
        var productoId = await CrearProductoAsync("Yerba");

        var creado = await _appService.CreateAsync(new CreateDespensaItemDto
        {
            ProductoId = productoId,
            Cantidad = 1,
            Unidad = "kg"
        });

        var guardado = await WithUnitOfWorkAsync(() => _itemRepository.GetAsync(creado.Id));
        guardado.PropietarioId.ShouldBe(_usuarioActualId);
        guardado.Estado.ShouldBe(EstadoItemDespensa.Disponible);
    }

    [Fact]
    public async Task Deberia_Actualizar_Cantidad_Unidad_Y_Fecha()
    {
        var item = await CrearItemAsync(_usuarioActualId, await CrearProductoAsync("Fideos"));
        var fecha = new DateTime(2026, 12, 1);

        var actualizado = await _appService.UpdateAsync(item, new UpdateDespensaItemDto
        {
            Cantidad = 2.5m,
            Unidad = "  kg  ",
            FechaVencimiento = fecha
        });

        actualizado.Cantidad.ShouldBe(2.5m);
        actualizado.Unidad.ShouldBe("kg");
        actualizado.FechaVencimiento.ShouldBe(fecha);
    }

    [Fact]
    public async Task No_Deberia_Aceptar_Cantidad_Negativa()
    {
        var item = await CrearItemAsync(_usuarioActualId, await CrearProductoAsync("Aceite"));

        await Should.ThrowAsync<AbpValidationException>(() =>
            _appService.UpdateAsync(item, new UpdateDespensaItemDto { Cantidad = -1 }));
    }

    [Fact]
    public async Task Deberia_Marcar_Item_Como_Consumido()
    {
        var item = await CrearItemAsync(_usuarioActualId, await CrearProductoAsync("Azúcar"));

        var resultado = await _appService.MarcarConsumidoAsync(item);

        resultado.Estado.ShouldBe(EstadoItemDespensa.Consumido);
    }

    [Fact]
    public async Task Deberia_Eliminar_Item_Y_Su_Advertencia()
    {
        var item = await CrearItemAsync(_usuarioActualId, await CrearProductoAsync("Harina"));
        await WithUnitOfWorkAsync(() => _advertenciaRepository.InsertAsync(new AdvertenciaVencimiento
        {
            DespensaItemId = item,
            Activa = true,
            Mensaje = "El producto vence hoy.",
            FechaVencimiento = DateTime.UtcNow.Date,
            FechaCalculo = DateTime.UtcNow
        }));

        await _appService.DeleteAsync(item);

        (await WithUnitOfWorkAsync(() => _itemRepository.FindAsync(item))).ShouldBeNull();
        (await WithUnitOfWorkAsync(() => _advertenciaRepository.FindAsync(x => x.DespensaItemId == item))).ShouldBeNull();
    }

    [Fact]
    public async Task No_Deberia_Modificar_Consumir_Ni_Eliminar_Un_Item_Ajeno()
    {
        var ajeno = await CrearItemAsync(Guid.NewGuid(), await CrearProductoAsync("Café"));

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _appService.UpdateAsync(ajeno, new UpdateDespensaItemDto { Cantidad = 10 }));
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _appService.MarcarConsumidoAsync(ajeno));
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _appService.DeleteAsync(ajeno));

        // Y el ítem ajeno quedó intacto
        var guardado = await WithUnitOfWorkAsync(() => _itemRepository.GetAsync(ajeno));
        guardado.Cantidad.ShouldBeNull();
        guardado.Estado.ShouldBe(EstadoItemDespensa.Disponible);
    }

    private async Task<Guid> CrearProductoAsync(string nombre)
    {
        var producto = new Producto(Guid.NewGuid(), nombre);
        await WithUnitOfWorkAsync(() => _productoRepository.InsertAsync(producto));
        return producto.Id;
    }

    private async Task<Guid> CrearItemAsync(Guid propietarioId, Guid productoId)
    {
        var item = new DespensaItem(Guid.NewGuid(), propietarioId, productoId, null, null, null);
        await WithUnitOfWorkAsync(() => _itemRepository.InsertAsync(item));
        return item.Id;
    }
}