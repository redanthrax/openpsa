using System.Text;
using Common.Audit;
using Common.Authentication;
using Common.Authorization;
using Common.Caching;
using Common.Database;
using Common.Modules;
using Common.Notifications;
using Common.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenPsa.Modules.Authentication;
using OpenPsa.Modules.Clients;
using OpenPsa.Modules.Contacts;
using OpenPsa.Modules.Dashboard;
using OpenPsa.Modules.Invoicing;
using OpenPsa.Modules.Notes;
using OpenPsa.Modules.Projects;
using OpenPsa.Modules.Security;
using OpenPsa.Modules.Settings;
using OpenPsa.Modules.Tickets;
using OpenPsa.Modules.TimeEntries;
using OpenPsa.Modules.Agreements;
using OpenPsa.Modules.Assets;
using OpenPsa.Modules.Email;
using OpenPsa.Modules.Expenses;
using OpenPsa.Modules.Sla;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try {
    Log.Information("Starting OpenPsa API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) => {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();

        if (ctx.HostingEnvironment.IsDevelopment())
            config.WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}");
        else
            config.WriteTo.Console(new CompactJsonFormatter());
    });

    var services = builder.Services;
    var configuration = builder.Configuration;

    var moduleAssemblies = new[] {
        typeof(AuthenticationModule).Assembly,
        typeof(ClientsModule).Assembly,
        typeof(ContactsModule).Assembly,
        typeof(ProjectsModule).Assembly,
        typeof(TicketsModule).Assembly,
        typeof(TimeEntriesModule).Assembly,
        typeof(InvoicingModule).Assembly,
        typeof(NotesModule).Assembly,
        typeof(SettingsModule).Assembly,
        typeof(DashboardModule).Assembly,
        typeof(SecurityModule).Assembly,
        typeof(AgreementsModule).Assembly,
        typeof(AssetsModule).Assembly,
        typeof(EmailModule).Assembly,
        typeof(ExpensesModule).Assembly,
        typeof(SlaModule).Assembly,
    };

    services.AddSingleton<IPermissionRegistry>(new PermissionRegistry());
    services.AddModules(moduleAssemblies);

    services.AddAuthenticationServices();
    services.AddAuditTrail();

    var jwtSecret = configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is required");
    var jwtIssuer = configuration["Jwt:Issuer"] ?? "openpsa";
    var jwtAudience = configuration["Jwt:Audience"] ?? "openpsa";

    services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents {
            OnMessageReceived = ctx => {
                var token = ctx.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token)) ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    })
    ;

    var googleClientId = configuration["Authentication:Google:ClientId"];
    var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        builder.Services.AddAuthentication().AddGoogle(options => {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });

    var msClientId = configuration["Authentication:Microsoft:ClientId"];
    var msClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
    if (!string.IsNullOrEmpty(msClientId) && !string.IsNullOrEmpty(msClientSecret))
        builder.Services.AddAuthentication().AddMicrosoftAccount(options => {
            options.ClientId = msClientId;
            options.ClientSecret = msClientSecret;
        });

    services.AddAuthorization(options => {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is required");

    var redisMultiplexer = services.AddRedisCache(configuration);

    services.AddDbContext<OpenPsaDbContext>((sp, opts) => {
        opts.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly("Api"));
        opts.UseAuditInterceptor(sp);
    });

    services.AddSignalR().AddStackExchangeRedis(
        configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis:ConnectionString is required"));

    services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres")
        .AddRedis(configuration["Redis:ConnectionString"]!, name: "redis");

    services.AddOpenApi();

    services.AddCors(options => options.AddDefaultPolicy(policy => {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    }));

    services.AddScoped<IPiiEncryptionService, PiiEncryptionService>();
    services.AddScoped<ITokenEncryptionService, DataProtectionTokenEncryptionService>();

    var dpBuilder = services.AddDataProtection()
        .SetApplicationName("OpenPsa");

    var dpKeysPath = configuration["DataProtection:KeysPath"];
    if (!string.IsNullOrEmpty(dpKeysPath)) {
        dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath));
    } else if (builder.Environment.IsDevelopment()) {
        var devKeysPath = Path.Combine(builder.Environment.ContentRootPath, ".keys");
        Directory.CreateDirectory(devKeysPath);
        dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(devKeysPath));
    }

    builder.Host.UseWolverine(opts => {
        opts.UseEntityFrameworkCoreTransactions();
        opts.PersistMessagesWithPostgresql(connectionString);

        foreach (var assembly in moduleAssemblies)
            opts.Discovery.IncludeAssembly(assembly);
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors();

    if (app.Environment.IsDevelopment()) {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseModules();

    app.MapModules();

    app.MapHub<NotificationHub>("/hubs/notifications");

    app.MapHealthChecks("/health");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException) {
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally {
    await Log.CloseAndFlushAsync();
}

return 0;
