[![Buil & Test .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml) [![CodeQL .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 10.0 Example App / API
## .Net 10.0 API using SQL Server with Entity Framework Core
## Unit Tests and Integration Tests
## Docker with Compose

Docker steps:

- switch to Linux containers

- copy `CF.Api/.env.example` to `CF.Api/.env` and set a strong `SA_PASSWORD`
  (this file is gitignored and supplies the SQL Server SA password to both the
  database container and the API connection string)

from the folder src run the below commands

- docker-compose -f CF.Api/docker-compose.yml build
- docker-compose -f CF.Api/docker-compose.yml up

http://localhost:8888/scalar/v1

## Local development (without Docker)

The committed `appsettings.json` intentionally omits the database password.
Provide a full connection string via user-secrets so no credential is committed:

- dotnet user-secrets --project CF.Api set "ConnectionStrings:DbConnection" "Data Source=localhost;Initial Catalog=CF;User ID=sa;Password=<your-password>;TrustServerCertificate=True;"

## Reusable application core

The Domain / Application / Infrastructure layers are host-agnostic: the whole service
graph (facade, domain services, repository, EF Core `DbContext`) is registered with a
single call, `IServiceCollection.AddCustomerCore(connectionString)`
(`CF.Customer.Infrastructure/DependencyInjection`). Both hosts consume the same core:

- **CF.Api** — the Web API.
- **CF.ConsoleApp** — a minimal console host that resolves `ICustomerFacade` and runs a
  create / read / list demo, showing the core can be reused outside the API.

Run the console demo. Its `launchSettings.json` sets `DOTNET_ENVIRONMENT=Development`,
so `dotnet run` loads the API's shared user-secrets store (same `UserSecretsId`) for the
connection string. Alternatively, override it with the `ConnectionStrings__DbConnection`
environment variable:

- dotnet run --project CF.ConsoleApp
