using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Configuration;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission> {
    public void Configure(EntityTypeBuilder<Permission> builder) {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Key).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Key).IsUnique();
        builder.HasIndex(p => p.Category);
    }
}
