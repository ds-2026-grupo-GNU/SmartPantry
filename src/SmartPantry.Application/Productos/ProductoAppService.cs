using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

// 1. Heredamos de CrudAppService, lo que nos regala GetListAsync, DeleteAsync y GetAsync.
public class ProductoAppService :
    CrudAppService<
        Producto,
        ProductoDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateProductoDto,
        UpdateProductoDto>,
    IProductoAppService
{
    public ProductoAppService(IRepository<Producto, Guid> repository)
        : base(repository)
    {
    }

    // 2. Sobreescribimos la creación para obligar a usar el constructor de nuestra Entidad
    public override async Task<ProductoDto> CreateAsync(CreateProductoDto input)
    {
        var producto = new Producto(GuidGenerator.Create(), input.Nombre);
        await Repository.InsertAsync(producto);

        return ObjectMapper.Map<Producto, ProductoDto>(producto);
    }

    // 3. Sobreescribimos la actualización para que respete el método SetNombre (y sus validaciones)
    public override async Task<ProductoDto> UpdateAsync(Guid id, UpdateProductoDto input)
    {
        // Buscamos la entidad original
        var producto = await Repository.GetAsync(id);

        // Aplicamos la regla de dominio (normaliza y valida antes de cambiar el estado)
        producto.SetNombre(input.Nombre);

        // Guardamos los cambios
        await Repository.UpdateAsync(producto);

        return ObjectMapper.Map<Producto, ProductoDto>(producto);
    }
}