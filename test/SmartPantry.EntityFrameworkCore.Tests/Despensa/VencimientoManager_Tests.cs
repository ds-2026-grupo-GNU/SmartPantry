using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Despensa;

public class VencimientoManager_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    // Fecha de referencia fija: todas las fechas de los tests son relativas a ella
    private static readonly DateTime Hoy = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);

    private readonly VencimientoManager _manager;
    private readonly IRepository<DespensaItem, Guid> _items;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertencias;

    public VencimientoManager_Tests()
    {
        _manager = GetRequiredService<VencimientoManager>();
        _items = GetRequiredService<IRepository<DespensaItem, Guid>>();
        _advertencias = GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>();
    }

    private Task<Guid> CrearItemAsync(DateTime? fechaVencimiento, EstadoItemDespensa estado = EstadoItemDespensa.Disponible)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var item = await _items.InsertAsync(new DespensaItem
            {
                PropietarioId = Guid.NewGuid(),
                ProductoId = Guid.NewGuid(),
                Estado = estado,
                FechaVencimiento = fechaVencimiento
            }, autoSave: true);
            return item.Id;
        });
    }

    private Task ModificarItemAsync(Guid itemId, Action<DespensaItem> cambio)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var item = await _items.GetAsync(itemId);
            cambio(item);
            await _items.UpdateAsync(item, autoSave: true);
        });
    }

    private Task ProcesarAsync(Guid itemId)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var item = await _items.GetAsync(itemId);
            await _manager.ProcesarItemAsync(item, Hoy);
        });
    }

    private Task<AdvertenciaVencimiento?> ObtenerAdvertenciaAsync(Guid itemId)
    {
        return WithUnitOfWorkAsync(async () =>
            await _advertencias.FirstOrDefaultAsync(x => x.DespensaItemId == itemId));
    }

    [Fact]
    public async Task Deberia_Crear_Advertencia_Dentro_Del_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(2));

        await ProcesarAsync(itemId);

        var advertencia = await ObtenerAdvertenciaAsync(itemId);
        advertencia.ShouldNotBeNull();
        advertencia.Activa.ShouldBeTrue();
        advertencia.FechaVencimiento.ShouldBe(Hoy.AddDays(2));
        advertencia.Mensaje.ShouldBe("El producto vencerá el 17/10/2026.");
    }

    [Fact]
    public async Task Deberia_Crear_Advertencia_Exactamente_En_El_Limite_Del_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(DespensaConsts.DiasUmbralAvisoVencimiento));

        await ProcesarAsync(itemId);

        var advertencia = await ObtenerAdvertenciaAsync(itemId);
        advertencia.ShouldNotBeNull();
        advertencia.Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task No_Deberia_Crear_Advertencia_Fuera_Del_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(DespensaConsts.DiasUmbralAvisoVencimiento + 1));

        await ProcesarAsync(itemId);

        (await ObtenerAdvertenciaAsync(itemId)).ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Informar_Que_Vence_Hoy()
    {
        var itemId = await CrearItemAsync(Hoy);

        await ProcesarAsync(itemId);

        var advertencia = await ObtenerAdvertenciaAsync(itemId);
        advertencia.ShouldNotBeNull();
        advertencia.Mensaje.ShouldBe("El producto vence hoy.");
    }

    [Fact]
    public async Task Deberia_Informar_Producto_Vencido()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(-1));

        await ProcesarAsync(itemId);

        var advertencia = await ObtenerAdvertenciaAsync(itemId);
        advertencia.ShouldNotBeNull();
        advertencia.Activa.ShouldBeTrue();
        advertencia.Mensaje.ShouldBe("¡El producto está vencido!");
    }

    [Fact]
    public async Task No_Deberia_Crear_Advertencia_Sin_Fecha_De_Vencimiento()
    {
        var itemId = await CrearItemAsync(null);

        await ProcesarAsync(itemId);

        (await ObtenerAdvertenciaAsync(itemId)).ShouldBeNull();
    }

    [Fact]
    public async Task No_Deberia_Crear_Advertencia_Para_Item_Consumido()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1), EstadoItemDespensa.Consumido);

        await ProcesarAsync(itemId);

        (await ObtenerAdvertenciaAsync(itemId)).ShouldBeNull();
    }

    [Fact]
    public async Task Deberia_Ser_Idempotente_Al_Procesar_Dos_Veces()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(2));

        await ProcesarAsync(itemId);
        var primera = await ObtenerAdvertenciaAsync(itemId);
        await ProcesarAsync(itemId);

        var advertencias = await WithUnitOfWorkAsync(async () =>
            await _advertencias.GetListAsync(x => x.DespensaItemId == itemId));
        advertencias.Count.ShouldBe(1);
        advertencias[0].Id.ShouldBe(primera!.Id);
        advertencias[0].FechaCalculo.ShouldBe(primera.FechaCalculo);
    }

    [Fact]
    public async Task Deberia_Actualizar_Advertencia_Cuando_Cambia_La_Fecha_Dentro_Del_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(2));
        await ProcesarAsync(itemId);
        var original = await ObtenerAdvertenciaAsync(itemId);

        await ModificarItemAsync(itemId, i => i.FechaVencimiento = Hoy.AddDays(1));
        await ProcesarAsync(itemId);

        var actualizada = await ObtenerAdvertenciaAsync(itemId);
        actualizada!.Id.ShouldBe(original!.Id);
        actualizada.Activa.ShouldBeTrue();
        actualizada.FechaVencimiento.ShouldBe(Hoy.AddDays(1));
        actualizada.Mensaje.ShouldBe("El producto vencerá el 16/10/2026.");
    }

    [Fact]
    public async Task Deberia_Desactivar_Advertencia_Cuando_La_Fecha_Supera_El_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1));
        await ProcesarAsync(itemId);
        var original = await ObtenerAdvertenciaAsync(itemId);

        await ModificarItemAsync(itemId, i => i.FechaVencimiento = Hoy.AddDays(10));
        await ProcesarAsync(itemId);

        var desactivada = await ObtenerAdvertenciaAsync(itemId);
        desactivada!.Id.ShouldBe(original!.Id);
        desactivada.Activa.ShouldBeFalse();
    }

    [Fact]
    public async Task Deberia_Desactivar_Advertencia_Cuando_El_Item_Se_Consume()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1));
        await ProcesarAsync(itemId);
        var original = await ObtenerAdvertenciaAsync(itemId);

        await ModificarItemAsync(itemId, i => i.Estado = EstadoItemDespensa.Consumido);
        await ProcesarAsync(itemId);

        var desactivada = await ObtenerAdvertenciaAsync(itemId);
        desactivada!.Id.ShouldBe(original!.Id);
        desactivada.Activa.ShouldBeFalse();
    }

    [Fact]
    public async Task Deberia_Desactivar_Advertencia_Cuando_Se_Quita_La_Fecha_De_Vencimiento()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1));
        await ProcesarAsync(itemId);
        var original = await ObtenerAdvertenciaAsync(itemId);

        await ModificarItemAsync(itemId, i => i.FechaVencimiento = null);
        await ProcesarAsync(itemId);

        var desactivada = await ObtenerAdvertenciaAsync(itemId);
        desactivada!.Id.ShouldBe(original!.Id);
        desactivada.Activa.ShouldBeFalse();
    }

    [Fact]
    public async Task Deberia_Reactivar_Advertencia_Cuando_La_Fecha_Vuelve_Al_Umbral()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1));
        await ProcesarAsync(itemId);
        var original = await ObtenerAdvertenciaAsync(itemId);

        await ModificarItemAsync(itemId, i => i.FechaVencimiento = Hoy.AddDays(10));
        await ProcesarAsync(itemId);
        (await ObtenerAdvertenciaAsync(itemId))!.Activa.ShouldBeFalse();

        await ModificarItemAsync(itemId, i => i.FechaVencimiento = Hoy.AddDays(2));
        await ProcesarAsync(itemId);

        var reactivada = await ObtenerAdvertenciaAsync(itemId);
        reactivada!.Id.ShouldBe(original!.Id);
        reactivada.Activa.ShouldBeTrue();
        reactivada.FechaVencimiento.ShouldBe(Hoy.AddDays(2));
    }

    [Fact]
    public async Task Deberia_Seleccionar_Items_Consumidos_Solo_Si_Tienen_Advertencia_Activa()
    {
        var conAdvertenciaId = await CrearItemAsync(Hoy.AddDays(1));
        await ProcesarAsync(conAdvertenciaId);
        await ModificarItemAsync(conAdvertenciaId, i => i.Estado = EstadoItemDespensa.Consumido);
        var sinAdvertenciaId = await CrearItemAsync(Hoy.AddDays(1), EstadoItemDespensa.Consumido);

        var ids = await WithUnitOfWorkAsync(() => _manager.ObtenerIdsDeItemsAProcesarAsync());

        ids.ShouldContain(conAdvertenciaId);
        ids.ShouldNotContain(sinAdvertenciaId);
    }

    [Fact]
    public async Task No_Deberia_Permitir_Dos_Advertencias_Para_El_Mismo_Item()
    {
        var itemId = await CrearItemAsync(Hoy.AddDays(1));
        await WithUnitOfWorkAsync(async () =>
            await _advertencias.InsertAsync(NuevaAdvertencia(itemId), autoSave: true));

        await Should.ThrowAsync<DbUpdateException>(async () =>
            await WithUnitOfWorkAsync(async () =>
                await _advertencias.InsertAsync(NuevaAdvertencia(itemId), autoSave: true)));
    }

    private static AdvertenciaVencimiento NuevaAdvertencia(Guid itemId) => new()
    {
        DespensaItemId = itemId,
        Activa = true,
        Mensaje = "Prueba",
        FechaVencimiento = Hoy.AddDays(1),
        FechaCalculo = Hoy
    };
}
