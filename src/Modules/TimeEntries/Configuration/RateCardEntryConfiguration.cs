using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.TimeEntries.Models;

namespace OpenPsa.Modules.TimeEntries.Configuration;

public class RateCardEntryConfiguration : IEntityTypeConfiguration<RateCardEntry> {
    public void Configure(EntityTypeBuilder<RateCardEntry> builder) {
        builder.ToTable("RateCardEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ServiceType).IsRequired().HasMaxLength(256);
        builder.Property(e => e.HourlyRate).HasPrecision(18, 2);
        builder.Property(e => e.AfterHoursRate).HasPrecision(18, 2);
        builder.HasIndex(e => e.RateCardId);
    }
}
