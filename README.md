# LabInsight

LabInsight is a configurable Laboratory Operations Analytics Dashboard built with Angular, ASP.NET Core and PostgreSQL.

All laboratory data displayed by this application is synthetically generated. The project does not contain or represent real patient or medical data.

## Technology stack

- Angular 21, TypeScript, Angular Material
- ASP.NET Core 10 Web API, Entity Framework Core, C#
- PostgreSQL 16 (containerized)

## Project structure

```
lab-insight/
├── frontend/          Angular application
├── backend/           ASP.NET Core Web API
├── docker-compose.yml PostgreSQL 16
├── .env.example       Local database credential template
└── README.md
```

## Local development requirements

- Fedora Linux (or another Linux distribution with Compose support)
- Podman or Docker, plus a Compose-compatible CLI (`docker compose` or `podman compose`)
- .NET SDK 10
- Node.js 20 and npm
- Angular CLI 21 (optional; `npx ng` from `frontend/` also works)

## Database

Development credentials live in `.env.example` and `appsettings.Development.json`. They are local defaults only. Copy `.env.example` to `.env` if you want to override Compose variables. Do not use these values in production.

From the project root:

```bash
# Start PostgreSQL (data is stored in the labinsight_pgdata volume)
docker compose up -d

# On Fedora with Podman, this equivalent also works:
podman compose up -d

# Stop PostgreSQL
docker compose stop

# Inspect the container
docker compose ps
docker logs labinsight-postgres
```

PostgreSQL is available at `localhost:5432`. Database name: `labinsight`.

## Backend

From `backend/`:

```bash
# Apply the EF Core migration (also runs automatically when the API starts)
dotnet ef database update --project LabInsight.Api.csproj

# Run the API
dotnet run --launch-profile http
```

If `dotnet-ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

The first API start applies pending migrations and seeds:

- laboratories and analysis categories
- graph types and graph data types
- approximately 10,000 synthetic `LabAnalysis` records
- the dashboard starts empty (graphs are created by the user)

Seeding is skipped when those records already exist.

- API: http://localhost:5080
- Swagger UI: http://localhost:5080/swagger

## Frontend

From `frontend/`:

```bash
npm install
npm start
```

Angular: http://localhost:4200

The dashboard loads graph items from `GET http://localhost:5080/api/graph-items`. The API base URL is configured in `frontend/src/environments/environment.ts`.
