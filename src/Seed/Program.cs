using Common.Authorization;
using Common.Database;
using Common.Modules;
using Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenPsa.Modules.Authentication;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;
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

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is required");

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
};

var services = new ServiceCollection();
services.AddSingleton<IPermissionRegistry, PermissionRegistry>();
services.AddModules(moduleAssemblies);
services.AddSingleton<IPiiEncryptionService, NullPiiEncryptionService>();
services.AddDbContext<OpenPsaDbContext>(opts =>
    opts.UseNpgsql(connectionString));

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();

Console.WriteLine("Applying migrations...");
await db.Database.MigrateAsync();

if (await db.Set<User>().AnyAsync()) {
    Console.WriteLine("Users already exist — skipping seed.");
    return 0;
}

Console.WriteLine("Seeding admin user (admin / admin)...");

db.Set<User>().Add(new User {
    Email = "admin@openpsa.dev",
    Name = "Admin",
    IsActive = true,
    IsSuperAdmin = true,
    LocalPasswordHash = PasswordHasher.Hash("admin"),
    CreatedAt = DateTime.UtcNow,
});

await db.SaveChangesAsync();
Console.WriteLine("Done.");
return 0;

// Minimal no-op PII encryption for the seed tool (no sensitive data being written)
internal sealed class NullPiiEncryptionService : IPiiEncryptionService {
    public string Encrypt(string plaintext) => plaintext;
    public string Decrypt(string ciphertext) => ciphertext;
}
