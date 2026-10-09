using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Despensa;

public class VencimientoManager : DomainService
{
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciasRepository;

    public VencimientoManager(IRepository<AdvertenciaVencimiento, Guid> advertenciasRepository)
    {
        _advertenciasRepository = advertenciasRepository;
    }

    public async Task ProcesarItemAsync(DespensaItem item)
    {
        // Regla: Consideramos la fecha UTC pura para estandarizar el cálculo[cite: 11]
        var hoy = DateTime.UtcNow.Date;
        var umbral = hoy.AddDays(3); // Umbral de aviso documentado: 3 días

        // Buscar si ya existe una advertencia para este ítem
        var advertencia = await _advertenciasRepository.FirstOrDefaultAsync(x => x.DespensaItemId == item.Id);

        // Regla: Si el ítem no tiene fecha o ya no está disponible (consumido/desechado)[cite: 10]
        if (!item.FechaVencimiento.HasValue || item.Estado != EstadoItemDespensa.Disponible)
        {
            if (advertencia != null && advertencia.Activa)
            {
                // Regla: Desactivar la advertencia cuando deja de corresponder[cite: 10]
                advertencia.Activa = false;
                advertencia.Mensaje = "Ítem sin fecha o ya no disponible.";
                await _advertenciasRepository.UpdateAsync(advertencia);
            }
            return;
        }

        var fechaVenc = item.FechaVencimiento.Value.Date;
        bool necesitaAdvertencia = fechaVenc <= umbral;
        string nuevoMensaje = string.Empty;

        if (necesitaAdvertencia)
        {
            if (fechaVenc < hoy) nuevoMensaje = "¡El producto está vencido!"; // Vencido[cite: 10]
            else if (fechaVenc == hoy) nuevoMensaje = "El producto vence hoy."; // Día límite[cite: 10]
            else nuevoMensaje = $"El producto vencerá el {fechaVenc:dd/MM/yyyy}.";
        }

        if (necesitaAdvertencia)
        {
            if (advertencia == null)
            {
                // Regla: Guardar una advertencia de vencimiento nueva[cite: 10]
                advertencia = new AdvertenciaVencimiento
                {
                    DespensaItemId = item.Id,
                    Activa = true,
                    Mensaje = nuevoMensaje,
                    FechaCalculo = DateTime.UtcNow
                };
                await _advertenciasRepository.InsertAsync(advertencia);
            }
            else
            {
                // Regla: Actualizarla si cambia la fecha[cite: 10]
                advertencia.Activa = true;
                advertencia.Mensaje = nuevoMensaje;
                advertencia.FechaCalculo = DateTime.UtcNow;
                await _advertenciasRepository.UpdateAsync(advertencia);
            }
        }
        else
        {
            if (advertencia != null && advertencia.Activa)
            {
                // Regla: Desactivar si la fecha es lejana y ya no aplica el umbral[cite: 10]
                advertencia.Activa = false;
                advertencia.Mensaje = "El producto tiene fecha lejana.";
                await _advertenciasRepository.UpdateAsync(advertencia);
            }
        }
    }
}