using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class BusinessHoursCalendarConfiguration : IEntityTypeConfiguration<BusinessHoursCalendar> {
    public void Configure(EntityTypeBuilder<BusinessHoursCalendar> builder) {
        builder.ToTable("BusinessHoursCalendars");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.TimeZoneId).IsRequired().HasMaxLength(100);
        builder.HasMany(c => c.Schedules).WithOne().HasForeignKey(s => s.CalendarId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Holidays).WithOne().HasForeignKey(h => h.CalendarId).OnDelete(DeleteBehavior.Cascade);
    }
}
