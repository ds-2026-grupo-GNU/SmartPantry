using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Despensa;

public class VencimientoManager : DomainService
{
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciasRepository;
    private readonly IRepository<DespensaItem, Guid> _itemsRepository;

    public VencimientoManager(
        IRepository<AdvertenciaVencimiento, Guid> advertenciasRepository,
        IRepository<DespensaItem, Guid> itemsRepository)
    {
        _advertenciasRepository = advertenciasRepository;
        _itemsRepository = itemsRepository;
    }

    // Regla de selección: se procesan los ítems disponibles y también los que tienen una
    // advertencia activa (aunque estén consumidos o desechados) para poder desactivarla.
    public async Task<List<Guid>> ObtenerIdsDeItemsAProcesarAsync()
    {
        var idsDisponibles = await AsyncExecuter.ToListAsync(
            (await _itemsRepository.GetQueryableAsync())
                .Where(x => x.Estado == EstadoItemDespensa.Disponible)
                .Select(x => x.Id));

        var idsConAdvertenciaActiva = await AsyncExecuter.ToListAsync(
            (await _advertenciasRepository.GetQueryableAsync())
                .Where(x => x.Activa)
                .Select(x => x.DespensaItemId));

        // TODO: si el ítem de una advertencia activa dejó de existir, el worker lo saltea (FindAsync null).
        return idsDisponibles.Union(idsConAdvertenciaActiva).ToList();
    }

    public async Task ProcesarItemAsync(DespensaItem item, DateTime fechaReferenciaUtc)
    {
        // Las fechas se comparan en UTC por día calendario
        var hoy = fechaReferenciaUtc.Date;
        var umbral = hoy.AddDays(DespensaConsts.DiasUmbralAvisoVencimiento);

        var advertencia = await _advertenciasRepository.FirstOrDefaultAsync(x => x.DespensaItemId == item.Id);

        string? motivoSinAdvertencia = null;
        if (item.Estado != EstadoItemDespensa.Disponible)
            motivoSinAdvertencia = "El ítem ya no está disponible.";
        else if (!item.FechaVencimiento.HasValue)
            motivoSinAdvertencia = "El ítem no tiene fecha de vencimiento.";
        else if (item.FechaVencimiento.Value.Date > umbral)
            motivoSinAdvertencia = "El producto tiene fecha lejana.";

        if (motivoSinAdvertencia != null)
        {
            // Deja de corresponder: se desactiva y se conserva la fila para reactivarla luego
            if (advertencia != null && advertencia.Activa)
            {
                advertencia.Activa = false;
                advertencia.Mensaje = motivoSinAdvertencia;
                await _advertenciasRepository.UpdateAsync(advertencia);
            }
            return;
        }

        var fechaVenc = item.FechaVencimiento!.Value.Date;
        string mensaje;
        if (fechaVenc < hoy) mensaje = "¡El producto está vencido!";
        else if (fechaVenc == hoy) mensaje = "El producto vence hoy.";
        else mensaje = $"El producto vencerá el {fechaVenc:dd/MM/yyyy}.";

        if (advertencia == null)
        {
            await _advertenciasRepository.InsertAsync(new AdvertenciaVencimiento
            {
                DespensaItemId = item.Id,
                Activa = true,
                Mensaje = mensaje,
                FechaVencimiento = fechaVenc,
                FechaCalculo = DateTime.UtcNow
            });
        }
        else if (!advertencia.Activa || advertencia.FechaVencimiento != fechaVenc || advertencia.Mensaje != mensaje)
        {
            advertencia.Activa = true;
            advertencia.Mensaje = mensaje;
            advertencia.FechaVencimiento = fechaVenc;
            advertencia.FechaCalculo = DateTime.UtcNow;
            await _advertenciasRepository.UpdateAsync(advertencia);
        }
        // Si ya está activa, con la misma fecha y mensaje, no se escribe (idempotente)
    }
}
