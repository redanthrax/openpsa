using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Common.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;
using OpenPsa.Modules.Sla.Models;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Api.Tests;

public class OpenPsaFactory : WebApplicationFactory<Program>, IAsyncLifetime {
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7")
        .Build();

    private const string JwtSecret = "this-is-a-test-secret-key-that-is-long-enough-for-hmac256!";

    public async Task InitializeAsync() {
        await _postgres.StartAsync();
        await _redis.StartAsync();
    }

    public new async Task DisposeAsync() {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("Redis:ConnectionString", _redis.GetConnectionString());
        builder.UseSetting("Jwt:Secret", JwtSecret);
        builder.UseSetting("Jwt:Issuer", "openpsa-test");
        builder.UseSetting("Jwt:Audience", "openpsa-test");
    }

    protected override IHost CreateHost(IHostBuilder builder) {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        db.Database.Migrate();
        SeedTestData(db);
        return host;
    }

    public Guid SeededAdminUserId { get; private set; }

    private void SeedTestData(OpenPsaDbContext db) {
        if (!db.Set<User>().Any(u => u.Email == "admin@openpsa.dev")) {
            var admin = new User {
                Email = "admin@openpsa.dev",
                Name = "Admin",
                IsActive = true,
                IsSuperAdmin = true,
                LocalPasswordHash = PasswordHasher.Hash("admin"),
                CreatedAt = DateTime.UtcNow,
            };
            db.Set<User>().Add(admin);
            db.SaveChanges();
            SeededAdminUserId = admin.Id;
        } else {
            SeededAdminUserId = db.Set<User>().First(u => u.Email == "admin@openpsa.dev").Id;
        }

        if (!db.Set<SlaPolicy>().Any()) {
            db.Set<SlaPolicy>().AddRange(
                new SlaPolicy { Name = "Standard", IsDefault = true, CreatedAt = DateTime.UtcNow },
                new SlaPolicy { Name = "Premium", CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
        }
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null, string? role = null) {
        var client = CreateClient();
        var token = GenerateToken(userId ?? Guid.NewGuid(), role ?? "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public HttpClient CreateClientWithoutSla(Guid? userId = null) {
        var client = CreateClient();
        var token = GenerateTokenWithoutSlaPerms(userId ?? Guid.NewGuid(), "User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public string GenerateExpiredToken(Guid userId, string role) {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("internal_user_id", userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("permissions", "clients.list,clients.view,clients.create,clients.update,clients.delete," +
                "sites.list,sites.view,sites.create,sites.update,sites.delete," +
                "time-entries.list,time-entries.view,time-entries.create,time-entries.update,time-entries.delete," +
                "rate-cards.list,rate-cards.view,rate-cards.create,rate-cards.update,rate-cards.delete," +
                "projects.list,projects.view,projects.create,projects.update,projects.delete," +
                "tickets.list,tickets.view,tickets.create,tickets.update,tickets.delete," +
                "ticket-queues.list,ticket-queues.view,ticket-queues.create,ticket-queues.update,ticket-queues.delete," +
                "invoices.list,invoices.view,invoices.create,invoices.update,invoices.delete,sla-policies.list,sla-policies.view,sla-policies.create,sla-policies.update,sla-policies.delete,sla.view-instances")
        };

        var token = new JwtSecurityToken(
            issuer: "openpsa-test",
            audience: "openpsa-test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateToken(Guid userId, string role) {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("internal_user_id", userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("permissions", "clients.list,clients.view,clients.create,clients.update,clients.delete," +
                "sites.list,sites.view,sites.create,sites.update,sites.delete," +
                "time-entries.list,time-entries.view,time-entries.create,time-entries.update,time-entries.delete," +
                "rate-cards.list,rate-cards.view,rate-cards.create,rate-cards.update,rate-cards.delete," +
                "projects.list,projects.view,projects.create,projects.update,projects.delete," +
                "tickets.list,tickets.view,tickets.create,tickets.update,tickets.delete," +
                "ticket-queues.list,ticket-queues.view,ticket-queues.create,ticket-queues.update,ticket-queues.delete," +
                "invoices.list,invoices.view,invoices.create,invoices.update,invoices.delete,sla-policies.list,sla-policies.view,sla-policies.create,sla-policies.update,sla-policies.delete,sla.view-instances")
        };

        var token = new JwtSecurityToken(
            issuer: "openpsa-test",
            audience: "openpsa-test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateTokenWithoutSlaPerms(Guid userId, string role) {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("internal_user_id", userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("permissions", "clients.list,clients.view,clients.create,clients.update,clients.delete," +
                "sites.list,sites.view,sites.create,sites.update,sites.delete," +
                "time-entries.list,time-entries.view,time-entries.create,time-entries.update,time-entries.delete," +
                "rate-cards.list,rate-cards.view,rate-cards.create,rate-cards.update,rate-cards.delete," +
                "projects.list,projects.view,projects.create,projects.update,projects.delete," +
                "tickets.list,tickets.view,tickets.create,tickets.update,tickets.delete," +
                "ticket-queues.list,ticket-queues.view,ticket-queues.create,ticket-queues.update,ticket-queues.delete," +
                "invoices.list,invoices.view,invoices.create,invoices.update,invoices.delete")
        };

        var token = new JwtSecurityToken(
            issuer: "openpsa-test",
            audience: "openpsa-test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
