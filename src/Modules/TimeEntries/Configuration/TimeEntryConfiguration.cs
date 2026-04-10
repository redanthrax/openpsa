using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.TimeEntries.Models;

namespace OpenPsa.Modules.TimeEntries.Configuration;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry> {
    public void Configure(EntityTypeBuilder<TimeEntry> builder) {
        builder.ToTable("TimeEntries");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Hours).HasPrecision(18, 2);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.HasIndex(t => t.ClientId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.TicketId);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.Date);
        builder.HasIndex(t => new { t.Billable, t.Invoiced });
    }
}
