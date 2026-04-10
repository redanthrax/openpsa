using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Configuration;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket> {
    public void Configure(EntityTypeBuilder<Ticket> builder) {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(512);
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.AssignedToUserId).HasMaxLength(64);
        builder.HasIndex(t => t.ClientId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.AssignedToUserId);
    }
}
