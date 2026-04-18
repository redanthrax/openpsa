# OpenPSA

Open-source Professional Services Automation for Managed Service Providers.

Built with ASP.NET 10, Blazor WASM, PostgreSQL, and Redis.

## Features

- Ticketing & service desk with SLA engine
- Client, contact, and site management
- Time tracking with rate cards
- Invoicing with line items
- Contract & agreement management
- Asset / CMDB tracking
- Email integration (IMAP/SMTP + Microsoft Graph)
- Role-based access control
- Real-time notifications via SignalR

## Quick Start

```bash
docker compose up -d
dotnet user-secrets set "Jwt:Secret" "dev-secret-change-in-production-min-32-chars" --project src/Api
dotnet run --project src/Seed
dotnet run --project src/Api
dotnet run --project src/Web
```

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for full setup instructions.

## License

[MIT](LICENSE)
