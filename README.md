[![.NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml)[![CodeQL .NET 9.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 9.0 Example API  

#NET 9.0 API Example

A simple .NET 9.0 API using SQL Server and Entity Framework Core.

#Features
	•	Unit and Integration Testing
	•	Docker Compose for deployment

#Docker Setup
	1.	Ensure Linux containers are enabled.
	2.	Run from the src directory:

docker-compose -f CF.Api/docker-compose.yml build
docker-compose -f CF.Api/docker-compose.yml up

#API Docs

Access the Swagger UI: http://localhost:8888/swagger/index.html