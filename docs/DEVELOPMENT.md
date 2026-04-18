# Development Guide

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Docker & Docker Compose | Latest | https://docs.docker.com/get-docker |
| Git | 2.x+ | https://git-scm.com |

Optional:
- A C# IDE (Rider, VS Code + C# Dev Kit, Visual Studio)
- [Scalar](https://scalar.com) is available at `/scalar/v1` when the API is running in development mode

---

## 1. Clone and Restore

```bash
git clone <repo-url> openpsa
cd openpsa
dotnet restore
```

---

## 2. Start Infrastructure

PostgreSQL 17 and Redis 7 run via Docker Compose. No application containers — just the databases.

```bash
docker compose up -d
```

This starts:
- **PostgreSQL** on `localhost:5432` (user: `postgres`, password: `postgres`, db: `openpsa`)
- **Redis** on `localhost:6379`

Verify they're healthy:

```bash
docker compose ps
```

Both services should show `healthy` status.

---

## 3. Configure Secrets

The API uses [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for sensitive configuration in development. The `UserSecretsId` is already configured in `src/Api/Api.csproj`.

### Required secrets

```bash
# JWT signing key (minimum 32 characters)
dotnet user-secrets set "Jwt:Secret" "your-development-secret-key-min-32-characters-long" --project src/Api
```

### Optional secrets

```bash
# Google OAuth (for Google login)
dotnet user-secrets set "Authentication:Google:ClientId" "<your-google-client-id>" --project src/Api
dotnet user-secrets set "Authentication:Google:ClientSecret" "<your-google-client-secret>" --project src/Api

# Microsoft OAuth (for Microsoft login)
dotnet user-secrets set "Authentication:Microsoft:ClientId" "<your-microsoft-client-id>" --project src/Api
dotnet user-secrets set "Authentication:Microsoft:ClientSecret" "<your-microsoft-client-secret>" --project src/Api
```

### What's already in `appsettings.Development.json`

These values are checked into source control and are fine for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=openpsa;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "openpsa",
    "Audience": "openpsa",
    "ExpiryMinutes": "480"
  }
}
```

> **Never put real secrets in appsettings files.** Use `dotnet user-secrets` for anything sensitive (API keys, OAuth credentials, production connection strings).

### View configured secrets

```bash
dotnet user-secrets list --project src/Api
```

---

## 4. Run Database Migrations & Seed

The `Seed` project applies all EF Core migrations and creates a default admin user.

```bash
dotnet run --project src/Seed
```

This will:
1. Apply all pending migrations to PostgreSQL
2. Create the admin user if no users exist:
   - **Email:** `admin@openpsa.dev`
   - **Password:** `admin`

> Run this again any time new migrations are added.

---

## 5. Run the Application

Open two terminals:

### Terminal 1 — API

```bash
dotnet run --project src/Api
```

The API starts on `http://localhost:5000` (or whichever port is configured). In development mode, the following endpoints are available:

| Endpoint | Description |
|----------|-------------|
| `/scalar/v1` | Interactive API documentation (Scalar) |
| `/openapi/v1.json` | OpenAPI spec |
| `/health` | Health check (Postgres + Redis) |
| `/hubs/notifications` | SignalR hub |

### Terminal 2 — Web (Blazor WASM)

```bash
dotnet run --project src/Web
```

The Blazor WebAssembly client starts on `http://localhost:5173`. It connects to the API using the `Api:BaseUrl` config key (defaults to `http://localhost:5000`).

### Or run both at once

```bash
dotnet run --project src/Api &
dotnet run --project src/Web &
```

---

## 6. Day-to-Day Development

### Build everything

```bash
dotnet build
```

### Adding a new EF Core migration

Migrations live in `src/Api/Migrations`. To add a new one after changing a model or configuration:

```bash
dotnet ef migrations add <MigrationName> --project src/Api --startup-project src/Api
```

Then apply it:

```bash
dotnet run --project src/Seed
```

### Running tests

```bash
dotnet test
```

### Checking API docs

Navigate to `http://localhost:5000/scalar/v1` in your browser while the API is running.

---

## Project Structure

```
openpsa/
├── docker-compose.yml          # PostgreSQL + Redis for local dev
├── Directory.Build.props        # Shared build settings
├── Directory.Packages.props     # Central package versioning
├── OpenPsa.slnx                 # Solution file (22 projects)
├── feature_plan.md              # Feature roadmap
├── docs/                        # Documentation
└── src/
    ├── Api/                     # ASP.NET API host (startup, DI, migrations)
    ├── Web/                     # Blazor WASM frontend (MudBlazor UI)
    ├── Seed/                    # Console app — migrations + admin user seed
    ├── Common/                  # Shared infrastructure (DB, auth, caching, audit)
    ├── Contracts/               # DTOs, enums, request/response types
    ├── IntegrationEvents/       # Cross-module events and queries (Wolverine)
    └── Modules/                 # Feature modules (vertical slices)
        ├── Authentication/      # JWT, OAuth, RBAC
        ├── Clients/             # Client management
        ├── Contacts/            # Contact management
        ├── Tickets/             # Service desk / ticketing
        ├── Projects/            # Project management
        ├── TimeEntries/         # Time tracking + rate cards
        ├── Invoicing/           # Invoices + line items
        ├── Agreements/          # MSAs, contracts, block hours
        ├── Sla/                 # SLA policies + breach tracking
        ├── Email/               # Email integration (IMAP/SMTP + Graph API)
        ├── Assets/              # CMDB / asset management
        ├── Expenses/            # Expense tracking
        ├── Notes/               # Markdown notes
        ├── Dashboard/           # Stats + activity feed
        ├── Settings/            # General settings
        └── Security/            # Roles + permissions
```

### Module anatomy

Each module follows the same vertical-slice pattern:

```
Modules/<Name>/
├── <Name>Module.cs              # IModule — registers permissions + services
├── Models/                      # EF Core entities
├── Configuration/               # EF Core entity configurations
├── Features/                    # Endpoint features (one folder per use case)
│   ├── Create<Entity>/
│   ├── Update<Entity>/
│   ├── Get<Entity>/
│   └── Integration/             # Wolverine handlers for cross-module queries
└── Services/                    # Background services, domain logic
```

---

## Configuration Reference

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `Jwt:Secret` | **Yes** | — | JWT signing key (min 32 chars). Use `dotnet user-secrets`. |
| `Jwt:Issuer` | No | `openpsa` | JWT token issuer |
| `Jwt:Audience` | No | `openpsa` | JWT token audience |
| `Jwt:ExpiryMinutes` | No | `480` | Token lifetime |
| `ConnectionStrings:DefaultConnection` | **Yes** | — | PostgreSQL connection string |
| `Redis:ConnectionString` | **Yes** | — | Redis connection string |
| `Authentication:Google:ClientId` | No | — | Google OAuth client ID |
| `Authentication:Google:ClientSecret` | No | — | Google OAuth client secret |
| `Authentication:Microsoft:ClientId` | No | — | Microsoft OAuth client ID |
| `Authentication:Microsoft:ClientSecret` | No | — | Microsoft OAuth client secret |
| `Cors:AllowedOrigins` | No | `["http://localhost:5173"]` | CORS allowed origins array |
| `Api:BaseUrl` | No | Web host address | API base URL (Web project) |

---

## Troubleshooting

### "Jwt:Secret is required"

You haven't set the JWT secret. Run:

```bash
dotnet user-secrets set "Jwt:Secret" "dev-secret-change-in-production-min-32-chars" --project src/Api
```

### "DefaultConnection is required"

Docker Compose isn't running or PostgreSQL is unreachable. Check:

```bash
docker compose ps
docker compose logs postgres
```

### "Redis:ConnectionString is required"

```bash
docker compose ps
docker compose logs redis
```

### Migrations out of date

```bash
dotnet run --project src/Seed
```

### Port conflicts

If `5432` or `6379` are already in use, stop the conflicting services or change the ports in `docker-compose.yml` and update `appsettings.Development.json` to match.

### Reset everything

```bash
docker compose down -v    # Destroys all data
docker compose up -d      # Fresh start
dotnet run --project src/Seed  # Re-migrate + re-seed
```
