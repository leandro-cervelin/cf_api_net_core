FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Containers log to stdout; the non-root user can't (and shouldn't) write log files into the app folder.
ENV NLog__rules__0__writeTo=console
# Run as the image's built-in unprivileged "app" user. It listens on 8080 (ASPNETCORE_HTTP_PORTS), not 80.
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CF.Api/CF.Api.csproj", "CF.Api/"]
RUN dotnet restore "CF.Api/CF.Api.csproj"
COPY . .
WORKDIR "/src/CF.Api"     
RUN dotnet build "CF.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CF.Api.csproj" -c Release -o /app/publish

# EF Core migration bundle: a single executable that applies pending migrations, run as its own deploy step
# (the API doesn't migrate on startup outside Development). It reads ConnectionStrings__DbConnection.
FROM build AS migrations
RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src
RUN dotnet ef migrations bundle --project CF.Migrations/CF.Migrations.csproj --startup-project CF.Api/CF.Api.csproj \
    --configuration Release --output /app/efbundle

FROM base AS migrator
WORKDIR /app
COPY --from=migrations /app/efbundle .
ENTRYPOINT ["./efbundle"]

# Last stage, so a plain `docker build` still produces the API image.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CF.Api.dll"]
