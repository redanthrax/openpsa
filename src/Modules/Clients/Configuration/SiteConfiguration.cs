using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Configuration;

public class SiteConfiguration : IEntityTypeConfiguration<Site> {
    public void Configure(EntityTypeBuilder<Site> builder) {
        builder.ToTable("Sites");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(300);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.City).HasMaxLength(200);
        builder.Property(s => s.State).HasMaxLength(200);
        builder.Property(s => s.PostalCode).HasMaxLength(50);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.Timezone).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.HasIndex(s => s.ClientId);
        builder.HasIndex(s => new { s.ClientId, s.IsPrimary });
    }
}
