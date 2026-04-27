# OpenPSA

Open-source Professional Services Automation application for tickets, time tracking, invoicing, projects, agreements, and SLA management.

OpenPSA is an open-source tool built for managed service providers and professional services teams. It handles core PSA workflows including client and contact management, support ticketing with SLA enforcement, time and expense tracking, project planning with milestones, agreement and contract handling, invoice generation from billable items, and dashboard overviews. The system supports role-based access control, real-time notifications, audit logging, and email integration. Modules include Authentication, Security, Settings, Clients, Contacts, Assets, Tickets, Sla, TimeEntries, Expenses, Agreements, Invoicing, Projects, Notes, Email, and Dashboard.

[![CI](https://github.com/redanthrax/openpsa/actions/workflows/ci.yml/badge.svg)](https://github.com/redanthrax/openpsa/actions/workflows/ci.yml)

## Tech Stack

- .NET 10 / ASP.NET Core: Backend framework for the API host
- Blazor WASM: Client-side web application framework
- EF Core + PostgreSQL: Object-relational mapping and primary database
- Redis: Caching and session storage
- Wolverine: Asynchronous messaging and event handling
- SignalR: Real-time web communications
- Serilog: Structured logging
- MudBlazor: UI component library for Blazor
- Scalar/OpenAPI: API documentation and exploration
- xUnit + Testcontainers: Unit and integration testing with containerized dependencies

## Prerequisites

- .NET 10 SDK: Required for building and running the application
- Docker or OrbStack: Needed to run PostgreSQL and Redis services via docker-compose
- Node.js: Not required for the basic setup or running the application; Blazor WASM builds are handled by .NET tools

## Quick Start

1. Clone the repository:
   ```
   git clone https://github.com/redanthrax/openpsa.git
   cd openpsa
   ```

2. Start the infrastructure services (PostgreSQL and Redis):
   ```
   docker compose up -d
   ```
   This exposes PostgreSQL on port 5432 and Redis on port 6379.

3. Seed the database with initial data and apply migrations:
   ```
   dotnet run --project src/Seed
   ```
   The seeder runs migrations and adds test data if no users exist.

4. In one terminal, start the API server:
   ```
   dotnet run --project src/Api
   ```
   The API will be available at http://localhost:5000.

5. In another terminal, start the web client:
   ```
   dotnet run --project src/Web
   ```
   The Blazor web application will be available at http://localhost:5001.

6. Open a browser and navigate to http://localhost:5001 to access the application.

For development, configure all sensitive values via [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets). Both the `Api` and `Seed` projects already have a `UserSecretsId` set.

```
# API
dotnet user-secrets init --project src/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=openpsa;Username=postgres;Password=postgres" --project src/Api
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379" --project src/Api
dotnet user-secrets set "Jwt:Secret" "dev-secret-change-in-production-min-32-chars" --project src/Api

# Seed (uses the same connection string)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=openpsa;Username=postgres;Password=postgres" --project src/Seed
```

OAuth and any other optional settings are documented in `docs/DEVELOPMENT.md`.

## Default Credentials

After running the seed project, an admin user is created with:
- Email: admin@openpsa.dev
- Password: admin

Log in at http://localhost:5001 using these credentials. Additional test users (e.g., sarah.chen@apextech.io / password) are also seeded for demonstration.

## Configuration

Configuration sources, in increasing priority order:

1. `appsettings.json` (committed) — non-sensitive defaults only (e.g. Serilog levels). **Never put secrets, connection strings, or signing keys here.**
2. `dotnet user-secrets` (per-developer, never committed) — the canonical place for everything sensitive in development.
3. Environment variables (e.g. `ConnectionStrings__DefaultConnection`, `Jwt__Secret`) — used in containers / CI / production.
4. Production secret stores — Azure Key Vault, AWS Secrets Manager, Kubernetes Secrets.

`appsettings.Development.json`, `appsettings.Production.json`, `appsettings.Staging.json`, `appsettings.Local.json`, and `secrets.json` are all gitignored. Do not create them — use user-secrets or env vars.

The Blazor WASM client (`src/Web/wwwroot/appsettings.json`) is the one exception: it ships to the browser and may only contain public values such as the API base URL.

Key configuration sections:

- **ConnectionStrings:DefaultConnection** — PostgreSQL connection string
- **Redis:ConnectionString** — Redis server connection
- **Jwt:Secret** — Symmetric key for JWT signing (minimum 32 characters)
- **Jwt:Issuer / Jwt:Audience** — JWT claims
- **Authentication:Google** / **Authentication:Microsoft** — OAuth client settings (ClientId, ClientSecret)
- **DataProtection:KeysPath** — File path for data protection keys (PII encryption)
- **Cors:AllowedOrigins** — array of allowed CORS origins

## Project Layout

The repository is organized as a modular monolith with shared infrastructure and vertical-slice modules.

```
src/
├── Api/                    ASP.NET Core 10 minimal API host with endpoints, middleware, and Wolverine integration
├── Web/                    Blazor WebAssembly client using MudBlazor 8 for UI components and API calls
├── Seed/                   Console application for idempotent database migrations and data seeding
├── Modules/                Vertical-slice feature modules, each handling a domain area (e.g., Tickets, Projects)
│   ├── Agreements/         Agreement and contract management with billing terms
│   ├── Assets/             CMDB for client assets and inventory
│   ├── Authentication/     User authentication and session management
│   ├── Clients/            Client account and relationship management
│   ├── Contacts/           Contact persons and communication tracking
│   ├── Dashboard/          Overview widgets and analytics
│   ├── Email/              IMAP/SMTP integration and inbound ticket parsing
│   ├── Expenses/           Expense tracking and reimbursement
│   ├── Invoicing/          Invoice generation from time and expenses
│   ├── Notes/              Notes and document attachments
│   ├── Projects/           Project planning, milestones, and task assignment
│   ├── Security/           Role-based access control and permissions
│   ├── Settings/           System configuration and general settings
│   ├── Sla/                SLA policies and business-hours calendar
│   ├── Tickets/            Ticketing system with queues and assignment
│   └── TimeEntries/        Time tracking with rate cards
├── Common/                 Shared abstractions for modules, database context, audit logging, and security
├── Contracts/              DTOs, permission keys, and contracts exposed to the client
└── IntegrationEvents/      Payloads for cross-module Wolverine events (no direct module references)

tests/
└── Api.Tests/              xUnit-based integration tests using Testcontainers for Postgres and Redis
```

Modules communicate solely through Contracts and IntegrationEvents to maintain loose coupling.

## Testing

Execute the full test suite with:
```
dotnet test OpenPsa.slnx
```

Tests cover unit and integration scenarios for the API and modules. Testcontainers spins up temporary PostgreSQL and Redis instances, so Docker must be running. The CI workflow (build-test job) runs the same suite against PostgreSQL 17 and Redis 7. A separate vulnerability-scan job checks dependencies.

Warnings are treated as errors during build. Coverage is focused on critical paths like authentication, permissions, and business logic.

## Architecture

OpenPSA follows a modular monolith pattern: a single deployable unit with 16 independent modules sharing one database. Each module owns its entities and endpoints, using Wolverine for asynchronous integration events.

For detailed information on module structure, persistence model, AuthN/Z flow, and coding conventions, see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Fork the repository, create a feature branch from master, implement changes following [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), add tests, and submit a pull request.