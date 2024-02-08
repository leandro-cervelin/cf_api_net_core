[![.NET 8.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml)[![CodeQL .NET 8.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 8.0 Example App / API
## .Net 8.0 API using SQL Server with Entity Framework Core
## Unit Tests and Integration Tests
## Docker with Compose

Docker steps:

- switch to Linux containers

from the folder src run the below commands

- docker-compose -f CF.Api/docker-compose.yml build
- docker-compose -f CF.Api/docker-compose.yml up

Swagger:

http://localhost:8888/swagger/index.html
