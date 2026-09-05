# SmartPantry
## Requisitos del Sistema

Para ejecutar este proyecto de forma local, es necesario contar con las siguientes herramientas instaladas:

*   **IDE:** Visual Studio 2022 o Visual Studio 2026, con la carga de trabajo *Desarrollo de ASP.NET y web*.
*   **Entorno de ejecución:** Node.js 24.15.0 o superior (Angular 22 no es compatible con Node 22.11.0).
*   **Gestor de paquetes Frontend:** Yarn 1.22.x (disponible globalmente en la consola).
*   **Motor de Base de Datos:** SQL Server Developer o SQL Server Express (instancia local, no usar base de datos remota).
*   **Gestor de Base de Datos:** SQL Server Management Studio (SSMS)[cite: 1].
*   **Herramientas CLI:** ABP Studio Desktop y Git[cite: 1].

---

## Configuración Local (Base de Datos)

El proyecto utiliza Entity Framework Core con SQL Server[cite: 1]. Antes de ejecutar las migraciones, debes configurar la cadena de conexión apuntando a tu instancia local de SQL Server.

Ubica y edita la sección `ConnectionStrings` en los siguientes dos archivos:
1. `src/SmartPantry.DbMigrator/appsettings.json`[cite: 1]
2. `src/SmartPantry.HttpApi.Host/appsettings.json`[cite: 1]

Reemplaza el valor de `Default` por tu cadena de conexión local usando autenticación de Windows. Por ejemplo:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB; Database=SmartPantry; Trusted_Connection=True"
  }
}
```
##Puesta en Marcha
Para levantar el proyecto desde cero, ejecuta los siguientes comandos desde la raíz del repositorio:
1. Restaurar dependencias globales
  abp install-libs
2. Ejecutar las migraciones iniciales (DbMigrator)
Este paso es obligatorio para crear las tablas base de infraestructura provistas por ABP (usuarios, roles, permisos, registros de auditoría) en tu base de datos local[cite: 1].
  dotnet run --project .\src\SmartPantry.DbMigrator
3. Iniciar el Backend (API Host)
  dotnet run --project .\src\SmartPantry.HttpApi.Host
4. Iniciar el Frontend (Angular)
La interfaz de Angular consume el HttpApi.Host mediante HTTP y no accede directamente a la base de datos[cite: 1]. Abre una nueva terminal, navega a la carpeta de Angular e inicia la aplicación:
  cd angular
  yarn install
  yarn start
##Verificación Continua (CI)
El grupo ejecuta los siguientes comandos de forma local para comprobar que el código compila y pasa las pruebas de forma exitosa antes de integrarlo mediante un Pull Request[cite: 1]:

Comandos de verificación Backend (.NET):
  dotnet build ./SmartPantry.slnx --configuration Debug --no-restore
  dotnet test ./SmartPantry.slnx --configuration Debug --no-build
Comandos de verificación Frontend (Angular):
  cd angular
  yarn build
  yarn test --watch=false --browsers=ChromeHeadless
