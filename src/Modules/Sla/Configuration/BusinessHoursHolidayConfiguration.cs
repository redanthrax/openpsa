using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class BusinessHoursHolidayConfiguration : IEntityTypeConfiguration<BusinessHoursHoliday> {
    public void Configure(EntityTypeBuilder<BusinessHoursHoliday> builder) {
        builder.ToTable("BusinessHoursHolidays");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(h => new { h.CalendarId, h.Date }).IsUnique();
    }
}
