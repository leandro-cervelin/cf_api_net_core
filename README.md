[![.NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml)[![CodeQL .NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 9.0 Example App / API  

A .NET 9.0 API leveraging **SQL Server** and **Entity Framework Core** for robust and efficient data operations.  

## Features  
- **Unit Tests** and **Integration Tests** for code quality assurance.  
- **Docker with Compose** for streamlined deployment.  

## Docker Setup  

1. **Switch to Linux containers**.  
2. From the `src` folder, run the following commands:  
   ```bash
   docker-compose -f CF.Api/docker-compose.yml build
   docker-compose -f CF.Api/docker-compose.yml up