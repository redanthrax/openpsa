# OpenPSA Architecture

## 1. Stack Overview

OpenPSA is built as a modular monolith using ASP.NET Core 10 for the backend API. Persistence is handled by Entity Framework Core 10 with the Npgsql provider connecting to PostgreSQL 17. Messaging and event handling use Wolverine 5.11.0, configured with PostgreSQL outbox pattern for reliable delivery. Caching is provided by StackExchange.Redis 2.8.16, serving both general cache and as the backplane for SignalR real-time notifications.

The frontend is a Blazor WebAssembly application hosted separately, using MudBlazor 8.15.0 for UI components. API documentation is generated via Microsoft.AspNetCore.OpenApi and rendered with Scalar.AspNetCore 2.12.12 for an interactive Swagger-like UI in development.

Authentication relies on JWT Bearer tokens (HS256 signed), with Serilog 4.2.0 for structured logging enriched with correlation IDs. Cross-cutting concerns like auditing, PII encryption, and UTC handling are integrated via EF Core interceptors and middleware.

Key packages:
- `Microsoft.EntityFrameworkCore` / `Npgsql.EntityFrameworkCore.PostgreSQL`: ORM and Postgres driver.
- `WolverineFx` / `WolverineFx.Postgresql` / `WolverineFx.EntityFrameworkCore`: CQRS and messaging.
- `StackExchange.Redis`: Caching and SignalR backplane.
- `Microsoft.AspNetCore.SignalR`: Real-time updates via `NotificationHub`.
- `MudBlazor`: Blazor UI library.
- `Serilog.AspNetCore`: Request logging.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: Token validation.
- `Microsoft.AspNetCore.DataProtection`: Encryption for PII and tokens.

The solution targets .NET 10, with central package management via `Directory.Packages.props`.

## 2. Project Layout

### src/

- **Common/Common.csproj**: Shared abstractions for modules, database, auditing, security, authorization, observability, and notifications.
- **Contracts/Contracts.csproj**: DTOs, request/response models, and permission keys exposed to the frontend.
- **IntegrationEvents/IntegrationEvents.csproj**: Event payloads for cross-module communication via Wolverine (e.g., `TicketCreated`, `ProjectUpdated`).
- **Api/Api.csproj**: ASP.NET Core 10 minimal API host, wiring modules, middleware, and Wolverine.
- **Web/Web.csproj**: Blazor WebAssembly client with MudBlazor UI, API client, and authentication state management.
- **Seed/Seed.csproj**: Idempotent database seeder for initial data (users, roles, permissions, sample entities).
- **Modules/Authentication/Authentication.csproj**: User authentication, JWT issuance, password hashing, and external provider integration.
- **Modules/Agreements/Agreements.csproj**: Contract and agreement management with billing terms.
- **Modules/Assets/Assets.csproj**: CMDB for client assets and inventory tracking.
- **Modules/Clients/Clients.csproj**: Client (organization) management, including sites and relationships.
- **Modules/Contacts/Contacts.csproj**: Contact persons associated with clients and projects.
- **Modules/Dashboard/Dashboard.csproj**: Overview metrics, charts, and recent activity summaries.
- **Modules/Email/Email.csproj**: IMAP/SMTP integration, inbound parsing, and outbound sending (Microsoft Graph support).
- **Modules/Expenses/Expenses.csproj**: Expense tracking and reimbursables linked to time/projects.
- **Modules/Invoicing/Invoicing.csproj**: Invoice generation from time/expenses, PDF export via QuestPDF.
- **Modules/Notes/Notes.csproj**: Free-form notes attached to entities (tickets, projects).
- **Modules/Projects/Projects.csproj**: Project hierarchy with milestones and task assignments.
- **Modules/Security/Security.csproj**: Role-based permissions, audit logs, and authorization handlers.
- **Modules/Settings/Settings.csproj**: System configuration, business hours, and global preferences.
- **Modules/Sla/Sla.csproj**: SLA policies with business-hours calendars and violation tracking.
- **Modules/Tickets/Tickets.csproj**: Service desk ticketing with queues, priorities, and status workflows.
- **Modules/TimeEntries/TimeEntries.csproj**: Time tracking with rate cards and billable/non-billable flags.

### tests/

- **Api.Tests/Api.Tests.csproj**: xUnit integration tests using WebApplicationFactory for end-to-end API scenarios, with Testcontainers for Postgres/Redis.

## 3. Modular Monolith Pattern

OpenPSA follows a modular monolith architecture: a single deployable process and database, but logically divided into independent vertical-slice modules. This enforces clear boundaries, reduces coupling, and eases future extraction into microservices if needed. Modules communicate solely via `Contracts/` (DTOs) and `IntegrationEvents/` (Wolverine messages), prohibiting direct references.

### Core Abstractions

The `Common.Modules` namespace defines `IModule`:

```csharp
public interface IModule
{
    void RegisterPermissions(IPermissionRegistry registry) { }
    void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Auto-discovers static IEndpointFeature implementations in the module assembly
        var assembly = GetType().Assembly;
        var featureTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpointFeature)) && t is { IsInterface: false, IsAbstract: false });

        foreach (var type in featureTypes)
        {
            var impl = type.GetMethod(nameof(IEndpointFeature.MapEndpoint),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            impl?.Invoke(null, [app]);
        }
    }
    void ConfigureDatabase(ModelBuilder modelBuilder, IPiiEncryptionService? piiEncryption = null) =>
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    void ConfigureServices(IServiceCollection services) { }
    void ConfigureMiddleware(IApplicationBuilder app) { }
}
```

- `RegisterPermissions`: Registers module-specific permission keys (e.g., `"tickets.create"`) into a global `PermissionRegistry` singleton.
- `MapEndpoints`: Scans the module assembly for `IEndpointFeature` implementations and invokes their static `MapEndpoint` methods to register minimal API routes.
- `ConfigureDatabase`: Applies EF Core configurations (e.g., `IEntityTypeConfiguration<T>`) from the module assembly, enabling PII encryption where configured.
- `ConfigureServices` / `ConfigureMiddleware`: Optional hooks for DI registrations and custom middleware.

`IEndpointFeature` is a marker for endpoint groups:

```csharp
public interface IEndpointFeature
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
```

Example usage in `TicketsModule`:

```csharp
public class TicketsModule : IModule
{
    public void RegisterPermissions(IPermissionRegistry registry)
    {
        registry.Register("tickets", "Ticket Management",
            ("tickets.list", "List tickets"),
            ("tickets.create", "Create ticket"),
            // ...
        );
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
    }
}
```

And for endpoints: `src/Modules/Tickets/Features/ListTickets/ListTicketsEndpoint.cs`:

```csharp
public class ListTicketsEndpoint : IEndpointFeature
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tickets", async (OpenPsaDbContext db, CancellationToken ct) =>
            await db.Tickets.ToListAsync(ct))
            .RequireAuthorization(new PermissionRequirement("tickets.list"))
            .WithTags("Tickets");
    }
}
```

### Auto-Discovery and Wiring

`ModuleExtensions` provides extension methods for discovery:

```csharp
public static IServiceCollection AddModules(this IServiceCollection services, Assembly[] assemblies)
{
    // Scans assemblies for concrete IModule implementations
    // Instantiates each, adds to DI as singleton
    // Calls ConfigureServices on each
    // Calls RegisterPermissions on each (using injected IPermissionRegistry)
    // ...
}

public static void ConfigureModuleDatabase(this ModelBuilder modelBuilder, IEnumerable<IModule> modules, IPiiEncryptionService? piiEncryption = null)
{
    foreach (var module in modules)
        module.ConfigureDatabase(modelBuilder, piiEncryption);
}

public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder app)
{
    foreach (var module in app.ServiceProvider.GetServices<IModule>())
        module.MapEndpoints(app);
    return app;
}
```

In `src/Api/Program.cs`:

```csharp
var moduleAssemblies = new[]
{
    typeof(AuthenticationModule).Assembly,
    typeof(TicketsModule).Assembly,
    // ... all 16 modules
};

services.AddSingleton<IPermissionRegistry>(new PermissionRegistry());
services.AddModules(moduleAssemblies);  // DI + permissions

// ... DbContext, Wolverine, etc.

var app = builder.Build();
app.UseModules();  // Middleware
app.MapModules();  // Endpoints
```

### Benefits

- **Clear Boundaries**: Modules are self-contained; no cross-references except contracts/events. Enforces single responsibility.
- **Future Extraction Path**: Each module can be extracted into a bounded context/microservice with minimal refactoring (move assembly, adjust messaging).
- **Discovery**: No manual registration; reflection scans ensure new modules are wired automatically.
- **Consistency**: Standardized patterns for endpoints (one feature class per use case), DB config, and permissions.

## 4. Request Lifecycle

A typical authenticated request flows as follows (ASCII wire diagram):

```
+-------------+       +-------------+       +-----------------+
|   Blazor    | ----> |  ApiClient  | ----> |   /api/tickets  |
|   Web (src/ |       | (HttpClient |       |   endpoint      |
|    Web)     |       |  w/ retry,  |       | (minimal API)   |
|             |       |  ITokenStore|       |                 |
+-------------+       +-------------+       +-----------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | CorrelationIdMiddleware  |
                                      | (sets TraceIdentifier,   |
                                      |  LogContext, X-Corr-Id   |
                                      |  header on response)     |
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | SerilogRequestLogging   |
                                      | (logs request duration,  |
                                      |  status, enriched w/ CID)|
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | JWT Authentication       |
                                      | (validates Bearer token  |
                                      |  from cookie/header, sets|
                                      |  User claims incl. perms)|
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | AuthorizationHandler     |
                                      | (PermissionAuthorization-|
                                      |  Handler checks claims   |
                                      |  for .RequirePermission( |
                                      |  "tickets.list"))        |
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | Endpoint Handler         |
                                      | (e.g., ListTicketsEndpt)|
                                      | (injects DbContext, etc.)|
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | Wolverine Command/Query  |
                                      | (e.g., ListTicketsQuery, |
                                      |  handled via EF)          |
                                      +-------------------------+
                                                    |
                                                    v
                                      +-------------------------+
                                      | EF DbContext -> Postgres |
                                      | (w/ interceptors: Audit, |
                                      |  UTC, PII encrypt)       |
                                      +-------------------------+
```

1. Blazor WASM sends HTTP request via `ApiClient` (adds auth token).
2. API receives at endpoint (minimal API route).
3. `CorrelationIdMiddleware` generates/reads CID, sets on context and logs.
4. `SerilogRequestLogging` wraps the request for logging.
5. `JwtBearer` authenticates, populates `HttpContext.User` with claims (incl. CSV permissions).
6. `PermissionAuthorizationHandler` evaluates requirements (e.g., `HasPermission("tickets.list")` parses claims).
7. Endpoint executes, often dispatching Wolverine query/command.
8. Handler uses `OpenPsaDbContext` (scoped), triggering EF operations.
9. Interceptors apply (audit changes, enforce UTC, encrypt PII).
10. Response flows back, with SignalR notifications if events published.

Unauthenticated paths (e.g., `/api/auth/login`) bypass auth/authorization via `.AllowAnonymous()`.

## 5. Cross-Cutting Concerns

### Auditing

All entity changes are audited via `AuditSaveChangesInterceptor` (EF Core `SaveChangesInterceptor`):

- Tracks `Added`/`Modified`/`Deleted` on auditable entities (configured via `IAuditConfiguration`).
- Captures old/new values (JSON serialized, excluding sensitive props), user context (`IUserContext`: ID/email/name), IP, User-Agent.
- Stores in `AuditEntries` table (entity name, ID, action, changes, timestamp).
- Triggered in `SavingChanges`/`SavedChanges` hooks; stamps `UpdatedAt` on modifications.
- Excludes `AuditEntry` itself and non-auditable types.

Registered in `Program.cs`: `opts.UseAuditInterceptor(sp);`

Example `AuditEntry`:

```csharp
public class AuditEntry
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public string? ChangedProperties { get; set; }  // JSON array
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### PII Encryption

Personally identifiable information (e.g., emails, names in contacts) is encrypted at rest using `PiiEncryptionService` (implements `IPiiEncryptionService`):

- Backed by ASP.NET Core `DataProtection` (AES-256-GCM, keys persisted to filesystem or Azure Blob in prod).
- Modules opt-in via EF configurations: `OwnsOne`/`HasColumn` with `.HasConversion(new EncryptedStringConverter(piiService))`.
- `ConfigureDatabase` passes `IPiiEncryptionService` to enable per-module encryption.
- Decryption transparent in queries via value converters.

`DataProtection` setup in `Program.cs`:

```csharp
var dpBuilder = services.AddDataProtection().SetApplicationName("OpenPsa");
if (!string.IsNullOrEmpty(dpKeysPath))
    dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath));
```

Token encryption uses `DataProtectionTokenEncryptionService` (similar, protector named "OpenPsa.Tokens").

### UTC DateTime Interceptor

`UtcDateTimeInterceptor` (EF Core `ISaveChangesInterceptor`) ensures all `DateTime` properties are UTC:

- In `SavingChanges`, sets `Kind = Utc` for non-specified `DateTime` values.
- Prevents timezone issues; all timestamps stored as UTC.

### Correlation ID

`CorrelationIdMiddleware` (early in pipeline):

- Reads `X-Correlation-Id` header or generates GUID.
- Sets `HttpContext.TraceIdentifier`, adds to `LogContext` for Serilog enrichment.
- Echoes in response header.
- Used in audit logs and request tracing.

### SignalR and Notifications

- `NotificationHub` (`/hubs/notifications`) for real-time updates (e.g., new ticket alerts).
- Backplane: Redis (`AddStackExchangeRedis`).
- Wolverine publishes `INotification` events, handled by `NotificationService` to broadcast via hub.
- Clients connect with JWT auth.

### Redis Cache

- `AddStackExchangeRedis` for `IDistributedCache`.
- Used for short-lived data (e.g., user permissions, session state).
- Config: `services.AddRedisCache(configuration);` (multiplexer singleton).

## 6. Authentication and Permissions

### Authentication

- **Local Login**: Email/password via `/api/auth/login`. Passwords hashed with PBKDF2 (100,000 iterations, SHA256, 16-byte salt):

```csharp
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        var result = new byte[48];
        salt.CopyTo(result, 0);
        hash.CopyTo(result, 16);
        return Convert.ToBase64String(result);
    }

    public static bool Verify(string password, string hash)
    {
        var bytes = Convert.FromBase64String(hash);
        var salt = bytes[..16];
        var stored = bytes[16..];
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(computed, stored);
    }
}
```

- **External Providers**: Google/Microsoft Account (OAuth2, configured optionally).
- **JWT Issuance**: `JwtService` generates HS256 tokens (symmetric key from config):

```csharp
public string GenerateToken(Guid userId, string email, string name, bool isSuperAdmin, IEnumerable<string> permissions)
{
    // ... claims: sub, email, name, internal_user_id, is_super_admin, permissions (CSV)
    var token = new JwtSecurityToken(
        issuer: issuer, audience: audience, claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

- Tokens stored in Blazor's `ITokenStore` (localStorage), sent as `access_token` cookie or `Authorization: Bearer`.
- Validation in `Program.cs`: `AddJwtBearer` with issuer/audience/lifetime checks; token from cookie fallback.

### Permissions

- **Registry**: `PermissionRegistry` singleton (`IPermissionRegistry`): Maps keys to descriptions/groups (e.g., `"tickets" => "Ticket Management"`).
- **Claims**: Tokens include `permissions` claim as CSV (e.g., `"tickets.list,tickets.create"`).
- **User Permissions**: Computed via `IPermissionService.GetUserPermissionsAsync` (roles -> permissions, super admin bypass).
- **Authorization**: Role-based via claims; fine-grained via `PermissionRequirement` and `PermissionAuthorizationHandler`:

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasPermission(requirement.PermissionKey))  // Parses CSV claim
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public record PermissionRequirement(string PermissionKey) : IAuthorizationRequirement;
```

- **Endpoint Checks**: `.RequireAuthorization(new PermissionRequirement("tickets.list"))`.
- **Super Admin**: Bypasses checks via `is_super_admin` claim.
- **Fallback**: Global policy requires authentication (`RequireAuthenticatedUser()`).

Permissions defined in module `RegisterPermissions`; exposed in `Contracts/<Module>/Permissions.cs`.

## 7. Data Access

- **Single Context**: `OpenPsaDbContext` (scoped) aggregates all entities; no per-module contexts.
- **Module Contributions**: In `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());

    if (_modules != null)
        modelBuilder.ConfigureModuleDatabase(_modules, _piiEncryption);  // Calls each module's ConfigureDatabase
}
```

- Each module provides `IEntityTypeConfiguration<T>` in its assembly (e.g., `TicketConfiguration` for indexes, relations, encryption).
- **Migrations**: Centralized in `src/Api/Migrations` (via `npgsql.MigrationsAssembly("Api")`). Run via `dotnet ef migrations add` in Api project; modules' configs auto-included.
- **Conventions**: Entities inherit `BaseEntity` (Id, CreatedAt/By, UpdatedAt/By). UTC enforced. Shadow props for auditing.
- **Connections**: Scoped per request; Wolverine uses transactional outbox.

Example module config:

```csharp
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.Property(t => t.Subject).HasMaxLength(500);
        builder.OwnsOne(t => t.Encryption, enc =>  // PII
            enc.Property(e => e.Email).HasConversion(new EncryptedStringConverter()));
        // Relations: HasOne<Client>, etc.
    }
}
```

## 8. Messaging

Wolverine handles CQRS and events:

- **Configuration** in `Program.cs`:

```csharp
builder.Host.UseWolverine(opts =>
{
    opts.UseEntityFrameworkCoreTransactions();  // Enlists in ambient Tx
    opts.PersistMessagesWithPostgresql(connectionString);  // Outbox table: `wolverine_outbox_messages`
    foreach (var assembly in moduleAssemblies)
        opts.Discovery.IncludeAssembly(assembly);  // Scans for handlers, messages
});
```

- **Patterns**:
  - **Queries**: Synchronous (e.g., `ListTicketsQuery` -> `IQueryHandler` -> EF read).
  - **Commands**: Async (e.g., `CreateTicketCommand` -> `ICommandHandler` -> EF write + publish events).
  - **Events**: Integration events (e.g., `TicketCreated` in `IntegrationEvents`) for cross-module (e.g., notify Email module).
- **Outbox**: Ensures at-least-once delivery; Postgres table stores pending messages, dispatched in background.
- **Transactions**: EF Tx + outbox atomic; no distributed Tx needed.
- **Discovery**: Scans module assemblies for `public class MyCommand;`, `public class MyHandler : CommandHandler<MyCommand>`.
- **Usage**: Endpoints dispatch: `await using var tx = await Wolverine.SendAsync(command);` or query via mediator-like.

No local queues; all via Postgres transport. Errors retried via Wolverine policies.

## 9. Frontend

The Blazor WASM app (`src/Web`) is a standalone host:

- **Program.cs**:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddMudServices();  // MudBlazor
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? builder.HostEnvironment.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
});  // Uses Microsoft.Extensions.Http.Resilience for retry (Polly-based)

builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();  // Persists JWT
builder.Services.AddScoped<JwtAuthenticationStateProvider>();  // Custom AuthStateProvider parsing JWT claims
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();  // Parses permissions CSV
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
```

- **ApiClient**: Wraps `HttpClient` with auth (adds token), retry policy (transient HTTP errors), and error handling (401 redirects to login).
- **ITokenStore**: Abstracts storage (localStorage in browser); loads/saves JWT.
- **Auth**: `JwtAuthenticationStateProvider` decodes token, builds `ClaimsPrincipal` with permissions for `<AuthorizeView>` and policy checks.
- **UI Components**: MudBlazor throughout (e.g., `MudDataGrid` for lists, `MudDialog` for forms). Custom wrappers like `DataGridPage<T>` for paginated CRUD.
- **Routing**: Blazor Router with lazy-loaded modules (e.g., `/tickets` loads Tickets.razor).
- **Real-Time**: SignalR client connects to `/hubs/notifications` for live updates (e.g., new tickets).
- **Theming**: `ThemeService` for MudBlazor themes (dark/light).

Build: `dotnet publish -c Release` for static files; serve via CDN or static host.

## 10. Local Development Workflow

1. **Infrastructure**:
   ```
   docker compose up -d  # Starts Postgres 17 (openpsa/openpsa) + Redis 7 (openpsa/redis)
   ```
   - `docker-compose.yml`: Volumes for data persistence; networks isolated.

2. **Secrets** (dev only):
   ```
   dotnet user-secrets set "Jwt:Secret" "dev-secret-change-in-production-min-32-chars" --project src/Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=openpsa;Username=postgres;Password=postgres" --project src/Api
   ```

3. **Seed Database**:
   ```
   dotnet run --project src/Seed  # Creates users (admin@openpsa.local / admin), roles, permissions, sample data
   ```

4. **Run Services** (separate terminals):
   ```
   dotnet run --project src/Api      # API at http://localhost:5000; Swagger at /scalar
   dotnet run --project src/Web      # Blazor at http://localhost:5001
   ```

- API config: `appsettings.Development.json` overrides (CORS origins: `http://localhost:5001`).
- Hot reload: Edit API/Web, auto-rebuilds.
- Migrations: `dotnet ef migrations add Initial --project src/Api` then `dotnet ef database update --project src/Api`.
- Health: `curl http://localhost:5000/health` (checks Postgres/Redis).

Prod: Docker images via `Microsoft.NET.Build.Containers`; Azure Blob for DataProtection keys.

## 11. Testing

Integration tests (`tests/Api.Tests`) use `WebApplicationFactory<Program>` via `OpenPsaFactory`:

- **OpenPsaFactory**: Custom factory spins up test host with in-memory? No, uses Testcontainers:
  - Postgres 17 + Redis 7 containers (started once per test run).
  - Overrides connection strings, JWT secret.
  - Migrates DB on host creation (`db.Database.Migrate()`).
  - Provides `CreateAuthenticatedClient()`: Generates JWT with full permissions for tests.

```csharp
public class OpenPsaFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:17").Build();
    private readonly RedisContainer _redis = new RedisBuilder().WithImage("redis:7").Build();

    public async Task InitializeAsync() => await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("Redis:ConnectionString", _redis.GetConnectionString());
        builder.UseSetting("Jwt:Secret", "this-is-a-test-secret-key-that-is-long-enough-for-hmac256!");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        db.Database.Migrate();
        return host;
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null, string? role = null)
    {
        var client = CreateClient();
        var token = GenerateToken(userId ?? Guid.NewGuid(), role ?? "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // GenerateToken: HS256 with claims (permissions CSV for all modules)
}
```

- **Shared Collection**: `IntegrationCollection` (xUnit `[CollectionDefinition("Integration")]`):

```csharp
[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<OpenPsaFactory>
{
    public const string Name = "Integration";
}
```

- All tests use `[Collection("Integration")]` to share one factory (avoids Wolverine outbox races, container startup overhead).
- Tests: End-to-end HTTP calls (e.g., `TicketsTests.cs`: POST /api/tickets, assert 201, verify DB).
- Assertions: FluentAssertions; no mocks (real DB/Wolverine).
- Run: `dotnet test tests/Api.Tests` (CI uses same via GitHub Actions).

This setup ensures reliable, isolated tests without port conflicts or data leaks.

