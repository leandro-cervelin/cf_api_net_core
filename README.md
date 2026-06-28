[![Buil & Test .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml) [![CodeQL .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 10.0 Example App / API
## .Net 10.0 API using SQL Server with Entity Framework Core
## Unit Tests and Integration Tests

## Project structure

The projects are split by responsibility and the references only point inward:

- `CF.Customer.Domain` - entities and business rules. Doesn't reference the other layers.
- `CF.Customer.Application` - the use cases (the facade) and DTOs. References Domain.
- `CF.Customer.Infrastructure` - EF Core, the `DbContext` and repositories. Implements the interfaces declared in the inner layers.
- `CF.Api` and `CF.ConsoleApp` - the entry points. They read config, wire up the services and call into the Application layer.
- `CF.Migrations` - the EF Core migrations, kept out of Infrastructure so they don't get in the way.

Because Domain and Application don't know about EF Core or ASP.NET, the business rules stay easy to test and the same core can run under more than one host. The API and the console app both register it the same way (`AddCustomerCore`); the console app is there mainly to prove that point.

Tests: `CF.Customer.UnitTest` and `CF.Api.UnitTest` run without a database. `CF.IntegrationTest` starts SQL Server with Testcontainers and exercises the real API.

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

## Console demo

`CF.ConsoleApp` resolves `ICustomerFacade` from the shared core and runs a small
create / read / list flow against the database. It uses the same `AddCustomerCore`
registration (in `CF.Customer.Infrastructure/DependencyInjection`) as the API.

Its `launchSettings.json` sets `DOTNET_ENVIRONMENT=Development`, so `dotnet run` picks up
the API's user-secrets (same `UserSecretsId`) for the connection string. You can also pass
it with the `ConnectionStrings__DbConnection` environment variable instead.

- dotnet run --project CF.ConsoleApp
