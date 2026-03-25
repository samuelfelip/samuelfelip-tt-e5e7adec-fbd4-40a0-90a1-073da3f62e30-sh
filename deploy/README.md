# Deploy snippets

Artifacts for **HighPerformance.Ingest.API** (.NET 10, ASP.NET Core, MediatR, EF Core, Npgsql, PostgreSQL, Swagger).

## Docker

Build from the **repository root**:

```bash
docker build -f deploy/Dockerfile -t highperf-ingest-api:latest .
```

`.dockerignore` at the repo root keeps `bin/`, `obj/`, `Tests/`, and secrets out of the build context.

## Docker Compose (API + PostgreSQL 16)

```bash
docker compose -f deploy/docker-compose.yml up --build
```

- API: `http://localhost:8080` (Swagger in **Development** — see `ASPNETCORE_ENVIRONMENT` in the compose file)
- PostgreSQL: `localhost:5432`

### Extra: instancia PostgreSQL con credenciales solicitadas

Para crear otra instancia aislada de PostgreSQL con usuario `GoSkalerTT` y contraseña `L4M4sS3gur4$$$`:

```bash
docker compose -f deploy/docker-compose.yml --profile goskaler up -d postgres_goskaler
```

- Host: `localhost`
- Port: `5433`
- Database: `high_performance_ingest`
- Username: `GoSkalerTT`
- Password: `L4M4sS3gur4$$$`

### EF Core migrations (before first request)

The API does not apply migrations on startup. From the host (with .NET SDK):

```bash
dotnet ef database update --project Src/HighPerformance.Ingest.Infrastructure --startup-project Src/HighPerformance.Ingest.API --connection "Host=localhost;Port=5432;Database=high_performance_ingest;Username=postgres;Password=postgres"
```

Or run the same command inside a one-off SDK container mounting this repo.

## Kubernetes

Files under `deploy/k8s/`:

- `deployment.yaml` — API Deployment (port **8080**)
- `service.yaml` — ClusterIP
- `secret.example.yaml` — template for `ConnectionStrings__PostgreSql`
- `job-db-migrate.example.yaml` — notes for migrations (runtime image has no `dotnet-ef`)

Replace `YOUR_REGISTRY/highperf-ingest-api:latest`, create secrets, then:

```bash
kubectl apply -f deploy/k8s/secret.example.yaml   # after editing — do not commit secrets
kubectl apply -f deploy/k8s/deployment.yaml
kubectl apply -f deploy/k8s/service.yaml
```

Readiness uses `GET /api/leaderboard?top=1` because Swagger is disabled in Production.

## Terraform

`deploy/terraform/` contains a **minimal scaffold**: variables and an output describing container environment variables. Uncomment a `required_providers` block and add resources for your cloud (Azure Container Apps, AWS ECS, GCP Cloud Run, etc.).

```bash
cd deploy/terraform
cp terraform.tfvars.example terraform.tfvars   # edit secrets locally — never commit
terraform init
terraform plan
```

## Files

| File | Role |
|------|------|
| `Dockerfile` | Multi-stage publish of `HighPerformance.Ingest.API` |
| `docker-compose.yml` | Postgres + API for local/demo |
| `env.example` | Env var names for Docker/K8s |
| `dockerignore.snippet` | Copy to repo root if you maintain `.dockerignore` elsewhere |
| `k8s/*.yaml` | Minimal cluster manifests |
| `terraform/*` | Variables + tfvars example |
