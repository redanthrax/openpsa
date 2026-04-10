using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Settings.Models;

namespace OpenPsa.Modules.Settings.Configuration;

public class GeneralSettingsConfiguration : IEntityTypeConfiguration<GeneralSettings> {
    public void Configure(EntityTypeBuilder<GeneralSettings> builder) {
        builder.ToTable("GeneralSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CompanyName).IsRequired().HasMaxLength(256);
        builder.Property(s => s.CompanyEmail).HasMaxLength(256);
        builder.Property(s => s.CompanyPhone).HasMaxLength(64);
        builder.Property(s => s.CompanyWebsite).HasMaxLength(512);
        builder.Property(s => s.DefaultCurrency).IsRequired().HasMaxLength(8);
    }
}
