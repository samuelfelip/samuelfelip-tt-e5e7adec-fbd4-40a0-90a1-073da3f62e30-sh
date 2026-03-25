# HighPerformance.Ingest.API

Boilerplate de .NET 10 Web API con arquitectura limpia, CQRS con MediatR, EF Core + PostgreSQL y leaderboard global: los scores se insertan en forma append-only usando `IScoreBulkIngestService` (binary `COPY` con Npgsql en PostgreSQL; EF en SQLite para tests).

**Endpoints de escritura:**

- `POST /api/scores` — un evento por petición (`RegisterScoreCommand`).
- `POST /api/scores/bulk` — varios eventos en una petición (`RegisterScoresBulkCommand`); mismo servicio de ingesta masiva y un único `COPY` / `SaveChanges` según el proveedor.

El tamaño máximo del lote bulk lo define `LeaderboardSettings:MaxScoreBatchSize` en `appsettings.json` (por defecto 10_000).

`timestamp` (cuando se envía) debe incluir zona horaria en formato ISO-8601 (por ejemplo `2026-03-24T12:00:00Z` o con offset). La API rechaza timestamps sin zona (`DateTimeKind.Unspecified`) y normaliza a UTC antes de persistir.

## Requisitos

- .NET 10 SDK
- PostgreSQL 14+ (recomendado)

## Docker Compose (que levanta)

El archivo `deploy/docker-compose.yml` levanta un entorno local con servicios preconfigurados:

- `postgres`: PostgreSQL 16 principal para la API.
  - Puerto host: `5432`
  - DB: `high_performance_ingest`
  - Usuario/clave por defecto: `postgres` / `postgres`
- `api`: contenedor de `HighPerformance.Ingest.API`.
  - Puerto host: `8080`
  - Usa `ConnectionStrings__PostgreSql=Host=postgres;Port=5432;...` para conectarse al servicio `postgres`.
  - Inyecta `LeaderboardSettings__WindowDays` y `LeaderboardSettings__MaxScoreBatchSize`.

Comando base:

```bash
docker compose -f deploy/docker-compose.yml up --build
```

Además, existe un perfil opcional `goskaler` para crear una segunda instancia PostgreSQL aislada:

- Servicio: `postgres_goskaler`
- Puerto host: `5433`
- Usuario: `GoSkalerTT`
- Password: `L4M4sS3gur4$$$`
- Database: `high_performance_ingest`

Comando:

```bash
docker compose -f deploy/docker-compose.yml --profile goskaler up -d postgres_goskaler
```

## Configuracion de conexion

Define la cadena de conexion en `Src/HighPerformance.Ingest.API/appsettings.json`:

```json
"ConnectionStrings": {
  "PostgreSql": "Host=localhost;Port=5432;Database=high_performance_ingest;Username=postgres;Password=postgres"
}
```

Tambien puedes usar la variable de entorno `POSTGRES_CONNECTION` para diseno/migraciones.

## Restaurar y compilar

```bash
dotnet restore HighPerformance.Ingest.slnx
dotnet build HighPerformance.Ingest.slnx
```

## Migraciones EF Core

Ejemplo para crear una nueva migracion:

```bash
dotnet ef migrations add NombreMigracion \
  --project Src/HighPerformance.Ingest.Infrastructure/HighPerformance.Ingest.Infrastructure.csproj \
  --startup-project Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj \
  --output-dir Persistence/Migrations
```

Aplicar migraciones:

```bash
dotnet ef database update \
  --project Src/HighPerformance.Ingest.Infrastructure/HighPerformance.Ingest.Infrastructure.csproj \
  --startup-project Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj
```

## Ejecutar API

```bash
dotnet run --project Src/HighPerformance.Ingest.API/HighPerformance.Ingest.API.csproj
```

## Ejecutar tests

```bash
dotnet test Tests/HighPerformance.Ingest.Tests/HighPerformance.Ingest.Tests.csproj
```
