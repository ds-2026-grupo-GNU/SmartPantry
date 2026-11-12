using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Productos;

// Al heredar de ICrudAppService, ABP nos obliga a cumplir con un contrato 
// que incluye los métodos Get, GetList (paginado), Create, Update y Delete.
public interface IProductoAppService :
    ICrudAppService<
        ProductoDto,
        Guid,
        PagedAndSortedResultRequestDto, // Este es el DTO que usa ABP para paginar (Skip, Take)
        CreateProductoDto,
        UpdateProductoDto>
{
}