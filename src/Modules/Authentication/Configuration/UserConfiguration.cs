using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(256);
        builder.Property(u => u.LocalPasswordHash).HasMaxLength(512);
        builder.Property(u => u.ExternalProvider).HasMaxLength(64);
        builder.Property(u => u.ExternalSubjectId).HasMaxLength(256);
        builder.Property(u => u.RoleIds).HasColumnType("jsonb");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => new { u.ExternalProvider, u.ExternalSubjectId }).IsUnique().HasFilter("\"ExternalSubjectId\" IS NOT NULL");
    }
}
