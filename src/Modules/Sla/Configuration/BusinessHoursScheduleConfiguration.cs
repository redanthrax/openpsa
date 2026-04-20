using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class BusinessHoursScheduleConfiguration : IEntityTypeConfiguration<BusinessHoursSchedule> {
    public void Configure(EntityTypeBuilder<BusinessHoursSchedule> builder) {
        builder.ToTable("BusinessHoursSchedules");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.CalendarId, s.DayOfWeek });
    }
}
