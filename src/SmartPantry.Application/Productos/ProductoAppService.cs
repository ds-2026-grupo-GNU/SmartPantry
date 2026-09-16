using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

public class ProductoAppService : SmartPantryAppService, IProductoAppService
{
    private readonly IRepository<Producto, Guid> _productoRepository;

    public ProductoAppService(IRepository<Producto, Guid> productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<ProductoDto> CreateAsync(CreateProductoDto input)
    {
        // El Id se genera internamente usando el IGuidGenerator provisto por la clase base
        var producto = new Producto(
            GuidGenerator.Create(),
            input.Nombre
        );

        await _productoRepository.InsertAsync(producto);

        return ObjectMapper.Map<Producto, ProductoDto>(producto);
    }

    public async Task<ProductoDto> GetAsync(Guid id)
    {
        // GetAsync devuelve automáticamente el error 404 si el Id no existe
        var producto = await _productoRepository.GetAsync(id);

        return ObjectMapper.Map<Producto, ProductoDto>(producto);
    }
}