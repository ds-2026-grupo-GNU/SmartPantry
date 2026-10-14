using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.CatalogoExterno;

public interface ICatalogoExternoAppService : IApplicationService
{
    Task<ProductoExternoDto> GetPorCodigoAsync(ConsultarProductoExternoDto input);
}
