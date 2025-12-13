FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CF.Api/CF.Api.csproj", "CF.Api/"]
RUN dotnet restore "CF.Api/CF.Api.csproj"
COPY . .
WORKDIR "/src/CF.Api"     
RUN dotnet build "CF.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CF.Api.csproj" -c Release -o /app/publish

SFROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CF.Api.dll"]
