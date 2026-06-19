# EventosVivos - Backend

Backend desarrollado para la prueba técnica de EventosVivos. La solución permite gestionar eventos, reservas, confirmación de pagos, cancelación de reservas, reportes de ocupación y validaciones de negocio asociadas.

## Instrucciones para ejecutar el proyecto localmente

### Prerrequisitos

* .NET SDK 10.
* SQL Server o Azure SQL disponible.
* Cadena de conexión configurada en:

```json
{
  "ConnectionStrings": {
    "EventosVivos": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
  }
}
```

La configuración se encuentra en:

```text
EventosVivos.Api/appsettings.json
```

### Comandos de ejecución

Desde la raíz de la solución, ejecutar:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project EventosVivos.Api\EventosVivos.Api.csproj
```

Para validar que la API se encuentra activa, se pueden usar los endpoints de health check:

```http
GET /api/health
GET /api/health/database
```

## Arquitectura elegida y justificación

El proyecto utiliza **Clean Architecture** con separación por capas para mantener bajo acoplamiento, alta mantenibilidad y facilidad de prueba.

La solución está organizada en las siguientes capas:

* **EventosVivos.Domain**: contiene el núcleo del negocio, entidades, value objects, agregados, reglas de dominio y validaciones propias del modelo.
* **EventosVivos.Application**: contiene los casos de uso, comandos, queries, contratos, validaciones de aplicación y orquestación de reglas que requieren consultar datos persistidos.
* **EventosVivos.Infrastructure**: implementa la persistencia con Entity Framework Core, SQL Server/Azure SQL, repositorios, unidad de trabajo, generación de códigos de reserva y servicios técnicos.
* **EventosVivos.Api**: expone los endpoints HTTP mediante Minimal APIs y conecta las solicitudes externas con los casos de uso de la aplicación.

También se aplica **CQRS con MediatR** para separar operaciones de escritura mediante comandos y operaciones de lectura mediante queries. Esta decisión permite organizar mejor los casos de uso, reducir responsabilidades en los endpoints y facilitar el crecimiento del sistema.

La arquitectura fue elegida porque permite:

* Separar reglas de negocio de detalles técnicos.
* Probar el dominio y los casos de uso sin depender directamente de la infraestructura.
* Reemplazar componentes técnicos como base de datos, repositorios o servicios externos con menor impacto.
* Mantener una estructura clara para funcionalidades como eventos, reservas, pagos, cancelaciones y reportes.

## Tecnologías utilizadas

* **.NET 10** como plataforma principal de desarrollo.
* **ASP.NET Core Minimal APIs** para la exposición de endpoints HTTP.
* **Clean Architecture** como estilo arquitectónico.
* **CQRS** para separar comandos y consultas.
* **MediatR** como mediador interno para comandos y queries.
* **Entity Framework Core** como ORM de persistencia.
* **Microsoft.EntityFrameworkCore.SqlServer** como proveedor para SQL Server/Azure SQL.
* **Microsoft.EntityFrameworkCore.Design** para soporte de tooling y migraciones.
* **SQL Server / Azure SQL** como motor de base de datos.
* **ProblemDetails** para estandarizar respuestas de error HTTP.
* **PBKDF2-SHA256** para hashing de contraseñas.
* **Pruebas unitarias** para validar reglas de dominio y casos principales del sistema.
