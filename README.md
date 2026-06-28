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
