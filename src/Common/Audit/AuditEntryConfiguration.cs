using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Audit;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry> {
    public void Configure(EntityTypeBuilder<AuditEntry> builder) {
        builder.ToTable("AuditEntries");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(128).IsRequired();
        builder.Property(a => a.Action).IsRequired();
        builder.Property(a => a.UserId).HasMaxLength(128);
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.UserName).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.UserAgent).HasMaxLength(512);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.UserId);
    }
}
