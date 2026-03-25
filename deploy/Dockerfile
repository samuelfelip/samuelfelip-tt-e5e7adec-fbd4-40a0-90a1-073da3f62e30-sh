# HighPerformance.Ingest.API — .NET 10 + ASP.NET Core Web API
# Build from repository root:
#   docker build -f deploy/Dockerfile -t highperf-ingest-api:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HighPerformance.Ingest.sln ./
COPY Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj Src/HighPerformance.Ingest.API/
COPY Src/HighPerformance.Ingest.Application/HighPerformance.Ingest.Application.csproj Src/HighPerformance.Ingest.Application/
COPY Src/HighPerformance.Ingest.Domain/HighPerformance.Ingest.Domain.csproj Src/HighPerformance.Ingest.Domain/
COPY Src/HighPerformance.Ingest.Infrastructure/HighPerformance.Ingest.Infrastructure.csproj Src/HighPerformance.Ingest.Infrastructure/

RUN dotnet restore Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj

COPY Src/ Src/

RUN dotnet publish Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

# Official aspnet image ships with non-root user `app`
USER app
ENTRYPOINT ["dotnet", "HighPerformance.Ingest.API.dll"]
