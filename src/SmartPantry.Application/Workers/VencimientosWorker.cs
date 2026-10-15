using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Despensa;

public class VencimientosWorker : AsyncPeriodicBackgroundWorkerBase
{
    // El constructor recibe el temporizador y la fábrica de "scopes" (alcances)
    public VencimientosWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Configuramos cada cuánto tiempo se despierta el worker.
        // Para pruebas locales lo ponemos en 30 segundos. En producción sería cada 12 o 24 horas.
        Timer.Period = 30000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Despertando: Iniciando escaneo de vencimientos...");

        // 1. RESOLVER DEPENDENCIAS DESDE EL ALCANCE (SCOPE)
        // Como el worker es "Singleton" (vive siempre), no podemos inyectarle un repositorio
        // directamente en el constructor. Debemos sacarlo del context específico de esta ejecución.
        var despensaRepository = workerContext.ServiceProvider.GetRequiredService<IRepository<DespensaItem, Guid>>();
        var vencimientoManager = workerContext.ServiceProvider.GetRequiredService<VencimientoManager>();
        var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        // Fecha de referencia única para toda la corrida (UTC, día calendario)
        var hoyUtc = DateTime.UtcNow.Date;

        // La selección de ítems es parte de la regla: la decide el Domain Service
        List<Guid> idsAProcesar;
        using (var uowSeleccion = uowManager.Begin())
        {
            idsAProcesar = await vencimientoManager.ObtenerIdsDeItemsAProcesarAsync();
            await uowSeleccion.CompleteAsync();
        }
        foreach (var itemId in idsAProcesar)
        {
            try
            {
                // 2. DELIMITAR UNA UNIDAD DE TRABAJO
                // Cada ítem se procesa en su propia transacción. Así, si uno falla, 
                // no deshace el guardado de los demás.
                using (var uow = uowManager.Begin())
                {
                    var item = await despensaRepository.FindAsync(itemId);
                    if (item == null)
                    {
                        continue;
                    }

                    // Delegamos la regla de negocio al Domain Service
                    await vencimientoManager.ProcesarItemAsync(item, hoyUtc);

                    // Confirmamos los cambios en la base de datos
                    await uow.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                // 3. REGISTRAR ERRORES SIN DETENER EJECUCIONES
                // Si el ítem falla (p. ej. violación del índice único por dos corridas superpuestas),
                // lo logueamos y seguimos: la próxima corrida corrige el estado.
                Logger.LogError(ex, "Error procesando el vencimiento para el ítem {ItemId}", itemId);
            }
        }

        Logger.LogInformation("Escaneo finalizado. Volviendo a dormir...");
    }
}