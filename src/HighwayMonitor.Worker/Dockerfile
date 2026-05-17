FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["HighwayMonitor.Worker/HighwayMonitor.Worker.csproj", "HighwayMonitor.Worker/"]
COPY ["../HighwayMonitor.Contracts/HighwayMonitor.Contracts.csproj", "HighwayMonitor.Contracts/"]

RUN dotnet restore "HighwayMonitor.Worker/HighwayMonitor.Worker.csproj"

COPY ["HighwayMonitor.Worker/", "HighwayMonitor.Worker/"]
COPY ["../HighwayMonitor.Contracts/", "HighwayMonitor.Contracts/"]

RUN dotnet publish "HighwayMonitor.Worker/HighwayMonitor.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HighwayMonitor.Worker.dll"]