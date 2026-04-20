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
        return host;
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null, string? role = null) {
        var client = CreateClient();
        var token = GenerateToken(userId ?? Guid.NewGuid(), role ?? "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateToken(Guid userId, string role) {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("internal_user_id", userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("permissions", "clients.list,clients.create,clients.update,clients.delete," +
                "time-entries.list,time-entries.create,time-entries.update,time-entries.delete," +
                "projects.list,projects.create,projects.update,projects.delete," +
                "tickets.list,tickets.create,tickets.update,tickets.delete," +
                "invoices.list,invoices.create,invoices.update,invoices.delete")
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
