FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["HighwayMonitor.Api/HighwayMonitor.Api.csproj", "HighwayMonitor.Api/"]
COPY ["../HighwayMonitor.Contracts/HighwayMonitor.Contracts.csproj", "HighwayMonitor.Contracts/"]

RUN dotnet restore "HighwayMonitor.Api/HighwayMonitor.Api.csproj"

COPY ["HighwayMonitor.Api/", "HighwayMonitor.Api/"]
COPY ["../HighwayMonitor.Contracts/", "HighwayMonitor.Contracts/"]

RUN dotnet publish "HighwayMonitor.Api/HighwayMonitor.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HighwayMonitor.Api.dll"]