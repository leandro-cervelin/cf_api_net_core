FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["CF.Api/CF.Api.csproj", "CF.Api/"]
RUN dotnet restore "CF.Api/CF.Api.csproj"
COPY . .
WORKDIR "/src/CF.Api"
RUN dotnet build "CF.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "CF.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CF.Api.dll"]

RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh