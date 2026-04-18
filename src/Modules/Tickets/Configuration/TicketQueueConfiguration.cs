using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Configuration;

public class TicketQueueConfiguration : IEntityTypeConfiguration<TicketQueue> {
    public void Configure(EntityTypeBuilder<TicketQueue> builder) {
        builder.ToTable("TicketQueues");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Name).IsRequired().HasMaxLength(256);
        builder.Property(q => q.Description).HasMaxLength(1000);
        builder.HasIndex(q => q.Name).IsUnique();
        builder.HasIndex(q => q.IsActive);
        builder.HasIndex(q => q.SortOrder);
    }
}
