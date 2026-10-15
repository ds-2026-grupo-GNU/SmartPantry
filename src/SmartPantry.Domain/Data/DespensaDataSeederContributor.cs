using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using SmartPantry.Despensa;

namespace SmartPantry.Data;

public class DespensaDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<DespensaItem, Guid> _despensaRepository;

    public DespensaDataSeederContributor(IRepository<DespensaItem, Guid> despensaRepository)
    {
        _despensaRepository = despensaRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Revisamos si ya hay datos para no sembrar dos veces
        if (await _despensaRepository.GetCountAsync() > 0)
        {
            return;
        }

        var propietarioFalso = Guid.NewGuid();
        var hoy = DateTime.UtcNow.Date;

        // Ítem 1: Vence hoy (Dentro del umbral)
        await _despensaRepository.InsertAsync(new DespensaItem
        {
            PropietarioId = propietarioFalso,
            ProductoId = Guid.NewGuid(),
            Estado = EstadoItemDespensa.Disponible,
            FechaVencimiento = hoy
        }, autoSave: true);

        // Ítem 2: Vence en 5 días (Fuera del umbral)
        await _despensaRepository.InsertAsync(new DespensaItem
        {
            PropietarioId = propietarioFalso,
            ProductoId = Guid.NewGuid(),
            Estado = EstadoItemDespensa.Disponible,
            FechaVencimiento = hoy.AddDays(5)
        }, autoSave: true);

        // Ítem 3: Sin fecha
        await _despensaRepository.InsertAsync(new DespensaItem
        {
            PropietarioId = propietarioFalso,
            ProductoId = Guid.NewGuid(),
            Estado = EstadoItemDespensa.Disponible,
            FechaVencimiento = null
        }, autoSave: true);
    }
}