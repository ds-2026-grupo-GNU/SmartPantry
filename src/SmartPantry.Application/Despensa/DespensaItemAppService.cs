using Microsoft.AspNetCore.Authorization;
using SmartPantry.Productos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace SmartPantry.Despensa;

[Authorize]
public class DespensaItemAppService : SmartPantryAppService, IDespensaItemAppService
{
    private readonly IRepository<DespensaItem, Guid> _itemRepository;
    private readonly IRepository<Producto, Guid> _productoRepository;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciaRepository;

    public DespensaItemAppService(
        IRepository<DespensaItem, Guid> itemRepository,
        IRepository<Producto, Guid> productoRepository,
        IRepository<AdvertenciaVencimiento, Guid> advertenciaRepository)
    {
        _itemRepository = itemRepository;
        _productoRepository = productoRepository;
        _advertenciaRepository = advertenciaRepository;
    }

    public async Task<PagedResultDto<DespensaItemDto>> GetListAsync(PagedResultRequestDto input)
    {
        var usuarioId = CurrentUser.GetId();

        var items = await _itemRepository.GetQueryableAsync();
        var productos = await _productoRepository.GetQueryableAsync();

        // Left join: un ítem cuyo producto no existe aparece igual, con NombreProducto = null (RF-09)
        var query =
            from item in items
            where item.PropietarioId == usuarioId
            join producto in productos on item.ProductoId equals producto.Id into productosDelItem
            from producto in productosDelItem.DefaultIfEmpty()
            orderby item.FechaVencimiento == null, item.FechaVencimiento
            select new DespensaItemDto
            {
                Id = item.Id,
                ProductoId = item.ProductoId,
                NombreProducto = producto != null ? producto.Nombre : null,
                Cantidad = item.Cantidad,
                Unidad = item.Unidad,
                FechaVencimiento = item.FechaVencimiento,
                Estado = item.Estado
            };

        var total = await AsyncExecuter.CountAsync(query);
        var pagina = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<DespensaItemDto>(total, pagina);
    }

    public async Task<DespensaItemDto> CreateAsync(CreateDespensaItemDto input)
    {
        // El producto tiene que existir (404 si no)
        var producto = await _productoRepository.GetAsync(input.ProductoId);

        var item = new DespensaItem(
            GuidGenerator.Create(),
            CurrentUser.GetId(),   // el dueño sale del token, nunca del DTO
            input.ProductoId,
            input.Cantidad,
            input.Unidad,
            input.FechaVencimiento);

        await _itemRepository.InsertAsync(item);

        return ToDto(item, producto.Nombre);
    }

    public async Task<DespensaItemDto> UpdateAsync(Guid id, UpdateDespensaItemDto input)
    {
        var item = await GetItemPropioAsync(id);

        item.ActualizarDatos(input.Cantidad, input.Unidad, input.FechaVencimiento);
        await _itemRepository.UpdateAsync(item);

        return ToDto(item, await GetNombreProductoAsync(item.ProductoId));
    }

    public async Task<DespensaItemDto> MarcarConsumidoAsync(Guid id)
    {
        var item = await GetItemPropioAsync(id);

        // Regla de negocio 6: no se elimina, cambia de estado
        item.MarcarConsumido();
        await _itemRepository.UpdateAsync(item);

        return ToDto(item, await GetNombreProductoAsync(item.ProductoId));
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetItemPropioAsync(id);

        // Se borra también su advertencia, para no dejarla huérfana para el worker
        await _advertenciaRepository.DeleteAsync(x => x.DespensaItemId == item.Id);
        await _itemRepository.DeleteAsync(item);
    }

    // Única regla de acceso: si el ítem no existe o es de otro usuario, responde 404.
    // Así no se revela si un Id ajeno existe, y todas las operaciones responden igual.
    private async Task<DespensaItem> GetItemPropioAsync(Guid id)
    {
        var item = await _itemRepository.FindAsync(id);

        if (item == null || item.PropietarioId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(DespensaItem), id);
        }

        return item;
    }

    private async Task<string?> GetNombreProductoAsync(Guid productoId)
    {
        var producto = await _productoRepository.FindAsync(productoId);
        return producto?.Nombre;
    }

    private static DespensaItemDto ToDto(DespensaItem item, string? nombreProducto)
    {
        return new DespensaItemDto
        {
            Id = item.Id,
            ProductoId = item.ProductoId,
            NombreProducto = nombreProducto,
            Cantidad = item.Cantidad,
            Unidad = item.Unidad,
            FechaVencimiento = item.FechaVencimiento,
            Estado = item.Estado
        };
    }
}
