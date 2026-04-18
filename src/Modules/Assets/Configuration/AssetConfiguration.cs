using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Assets.Models;

namespace OpenPsa.Modules.Assets.Configuration;

public class AssetConfiguration : IEntityTypeConfiguration<Asset> {
    public void Configure(EntityTypeBuilder<Asset> builder) {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(500);
        builder.Property(a => a.SerialNumber).HasMaxLength(200);
        builder.Property(a => a.Manufacturer).HasMaxLength(200);
        builder.Property(a => a.Model).HasMaxLength(200);
        builder.Property(a => a.OperatingSystem).HasMaxLength(200);
        builder.Property(a => a.IpAddress).HasMaxLength(100);
        builder.Property(a => a.MacAddress).HasMaxLength(100);
        builder.Property(a => a.Location).HasMaxLength(500);
        builder.Property(a => a.PurchasePrice).HasPrecision(18, 2);
        builder.HasIndex(a => a.ClientId);
        builder.HasIndex(a => a.SiteId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.SerialNumber);
        builder.HasIndex(a => a.WarrantyExpiry);
    }
}
