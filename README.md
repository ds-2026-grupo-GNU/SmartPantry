# SmartPantry

## Requisitos del Sistema

Para ejecutar este proyecto de forma local, es necesario contar con las siguientes herramientas instaladas:

* **IDE:** Visual Studio 2022 o Visual Studio 2026, con la carga de trabajo *Desarrollo de ASP.NET y web*.
* **Entorno de ejecución:** Node.js 24.15.0 o superior (Angular 22 no es compatible con Node 22.11.0).
* **Gestor de paquetes Frontend:** Yarn 1.22.x (disponible globalmente en la consola).
* **Motor de Base de Datos:** SQL Server Developer o SQL Server Express (instancia local, no usar base de datos remota).
* **Gestor de Base de Datos:** SQL Server Management Studio (SSMS)\[cite: 1].
* **Herramientas CLI:** ABP Studio Desktop y Git\[cite: 1].

\---

## Configuración Local (Base de Datos)

El proyecto utiliza Entity Framework Core con SQL Server\[cite: 1]. Antes de ejecutar las migraciones, debes configurar la cadena de conexión apuntando a tu instancia local de SQL Server.

Ubica y edita la sección `ConnectionStrings` en los siguientes dos archivos:

1. `src/SmartPantry.DbMigrator/appsettings.json`\[cite: 1]
2. `src/SmartPantry.HttpApi.Host/appsettings.json`\[cite: 1]

Reemplaza el valor de `Default` por tu cadena de conexión local usando autenticación de Windows. Por ejemplo:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\\\MSSQLLocalDB; Database=SmartPantry; Trusted\_Connection=True"
  }
}
```

\##Puesta en Marcha
Para levantar el proyecto desde cero, ejecuta los siguientes comandos desde la raíz del repositorio:

1. Restaurar dependencias globales
abp install-libs
2. Ejecutar las migraciones iniciales (DbMigrator)
Este paso es obligatorio para crear las tablas base de infraestructura provistas por ABP (usuarios, roles, permisos, registros de auditoría) en tu base de datos local\[cite: 1].
dotnet run --project .\\src\\SmartPantry.DbMigrator
3. Iniciar el Backend (API Host)
dotnet run --project .\\src\\SmartPantry.HttpApi.Host
4. Iniciar el Frontend (Angular)
La interfaz de Angular consume el HttpApi.Host mediante HTTP y no accede directamente a la base de datos\[cite: 1]. Abre una nueva terminal, navega a la carpeta de Angular e inicia la aplicación:

```
  cd angular
  yarn install
  yarn start
```

\##Verificación Continua (CI)
El grupo ejecuta los siguientes comandos de forma local para comprobar que el código compila y pasa las pruebas de forma exitosa antes de integrarlo mediante un Pull Request\[cite: 1]:

Comandos de verificación Backend (.NET):

```
  dotnet build ./SmartPantry.slnx --configuration Debug --no-restore
  dotnet test ./SmartPantry.slnx --configuration Debug --no-build
```

Comandos de verificación Frontend (Angular):

```
  cd angular
  yarn build
  yarn test --watch=false --browsers=ChromeHeadless
```



\## Decisiones Técnicas y de Negocio - Pantalla Mi Despensa (RF-11)



\### 1. Paginación y uso de `ListService`

La especificación técnica indica que la pantalla debe presentar los ítems de forma paginada\[cite: 1, 3]. Por ello, se implementó el `ListService` provisto por `@abp/ng.core`\[cite: 3]. Este servicio maneja nativamente el estado del listado (incluyendo paginación, ordenamiento y actualización), la suscripción a eventos de recarga (mediante `hookToQuery`) y la inyección transparente de los parámetros de consulta requeridos por el proxy generado (`DespensaItemService.getList`).



\### 2. Tratamiento de los Ítems Consumidos (RF-06)

De acuerdo a la regla de negocio 6, los ítems marcados como consumidos no se borran automáticamente de la base de datos\[cite: 3]. El equipo decidió \*\*conservarlos como historial\*\*. En la interfaz actual, al pulsar "Marcar como Consumido", el registro actualiza su estado interno en el BackEnd (cambiando de \*Disponible\* a \*Consumido\*). La vista principal de "Mi Despensa" se recarga automáticamente tras la operación. Queda a criterio de futuras iteraciones (fuera del alcance de RF-11) agregar un filtro visual o una pestaña separada para consultar el "Historial de Consumidos".



\### 3. Criterio de Fechas de Vencimiento

Las fechas de vencimiento en la tabla y en el formulario de edición se presentan formateadas de acuerdo a la configuración regional del navegador del usuario (Local Time)\[cite: 3]. Sin embargo, la persistencia en el BackEnd y el Worker de notificaciones del TP08 operan estrictamente en \*\*UTC\*\*\[cite: 3]. Se asume que el proxy de ABP y los interceptores de Angular manejan la conversión implícita de zona horaria al enviar y recibir los DTOs, garantizando que el Worker evalúe el vencimiento de manera consistente, independientemente del huso horario del cliente web.



\### 4. Estrategia de Manejo de Errores

Se optó por un manejo explícito de errores a nivel de componente para las operaciones de lectura (Listado)\[cite: 3]. Si la llamada inicial al BackEnd falla (ej: servidor caído o error 500), la suscripción captura el error, activa el flag `hasError = true` y detiene el indicador de carga (`isLoading = false`)\[cite: 3]. Esto renderiza un bloque de error localizado (`Pantry:Error`) que incluye un botón de "Reintentar" (`Pantry:Retry`)\[cite: 3], mejorando la usabilidad\[cite: 3].

Para las operaciones de escritura (crear, editar, eliminar), se delega el manejo del error al interceptor global predeterminado de ABP (`ErrorHandler`), el cual despliega automáticamente notificaciones tipo \*toast\* con los detalles (ej. 403 Forbidden o 400 Bad Request por validaciones del dominio)\[cite: 3].



\### 5. Autorización y Seguridad (Aislamiento de Usuario)

La interfaz oculta condicionalmente los botones para evitar envíos inválidos, pero \*\*la verdadera seguridad recae en el servidor\*\*\[cite: 1, 5]. El `AppService` de C# exige autenticación (`\[Authorize]`)\[cite: 1] y verifica que el `CreatorId` del registro coincida con el `ICurrentUser.Id` antes de ejecutar una lectura, actualización o eliminación\[cite: 1]. Cualquier intento de manipular un ítem ajeno (por ejemplo, alterando el ID en la petición HTTP) es interceptado por el BackEnd, devolviendo un error \*\*403 Forbidden\*\*\[cite: 1], tal como se comprobó y documentó mediante Swagger\[cite: 2, 4].

