using Common.Audit;
using Common.Modules;
using Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Common.Database;

public class OpenPsaDbContext : DbContext {
    private readonly IEnumerable<IModule>? _modules;
    private readonly IPiiEncryptionService? _piiEncryption;

    public OpenPsaDbContext(DbContextOptions<OpenPsaDbContext> options,
        IEnumerable<IModule>? modules = null,
        IPiiEncryptionService? piiEncryption = null)
        : base(options) {
        _modules = modules;
        _piiEncryption = piiEncryption;
    }

    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());

        if (_modules != null) {
            modelBuilder.ConfigureModuleDatabase(_modules, _piiEncryption);
        }
    }
}
