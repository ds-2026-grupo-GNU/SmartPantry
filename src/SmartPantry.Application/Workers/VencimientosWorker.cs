using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Domain.Repositories;

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

        // Solo traemos los ítems que están disponibles para no cargar la memoria en vano
        var itemsDisponibles = await despensaRepository.GetListAsync(x => x.Estado == EstadoItemDespensa.Disponible);

        foreach (var item in itemsDisponibles)
        {
            try
            {
                // 2. DELIMITAR UNA UNIDAD DE TRABAJO
                // Cada ítem se procesa en su propia transacción. Así, si uno falla, 
                // no deshace el guardado de los demás.
                using (var uow = uowManager.Begin())
                {
                    // Delegamos la regla de negocio al Domain Service
                    await vencimientoManager.ProcesarItemAsync(item);

                    // Confirmamos los cambios en la base de datos
                    await uow.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                // 3. REGISTRAR ERRORES SIN DETENER EJECUCIONES[cite: 10]
                // Si el ítem falla, lo logueamos pero el foreach continúa con el siguiente producto.
                Logger.LogError(ex, "Error procesando el vencimiento para el ítem {ItemId}", item.Id);
            }
        }

        Logger.LogInformation("Escaneo finalizado. Volviendo a dormir...");
    }
}