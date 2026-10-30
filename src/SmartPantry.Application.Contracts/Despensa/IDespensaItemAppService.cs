using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Despensa;

public interface IDespensaItemAppService : IApplicationService
{
    Task<PagedResultDto<DespensaItemDto>> GetListAsync(PagedResultRequestDto input);

    Task<DespensaItemDto> CreateAsync(CreateDespensaItemDto input);

    // RF-11: editar, consumir y eliminar
    Task<DespensaItemDto> UpdateAsync(Guid id, UpdateDespensaItemDto input);
    Task<DespensaItemDto> MarcarConsumidoAsync(Guid id);
    Task DeleteAsync(Guid id);
}