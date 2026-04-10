using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Configuration;

public class ClientConfiguration : IEntityTypeConfiguration<Client> {
    public void Configure(EntityTypeBuilder<Client> builder) {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Website).HasMaxLength(512);
        builder.Property(c => c.Phone).HasMaxLength(64);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Status);
    }
}
