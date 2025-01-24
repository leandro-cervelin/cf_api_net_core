[![.NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml)[![CodeQL .NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)  

# .NET 9.0 API Example

A .NET 9.0 API using SQL Server and Entity Framework Core.

## Features
- **Unit Test and Integration Testing** for quality assurance.
- **Docker Compose** for streamlined deployment.

## Prerequisites
- Docker installed and set to **Linux containers**.

## Getting Started

### Running the Application
1. Navigate to the `src` directory:
```bash
   
   cd src
```
2.	Build and start the Docker containers:
```bash
   
   docker-compose -f CF.Api/docker-compose.yml build
   docker-compose -f CF.Api/docker-compose.yml up
```

## API Documentation

Access the Swagger UI at:
http://localhost:8888/swagger/index.html