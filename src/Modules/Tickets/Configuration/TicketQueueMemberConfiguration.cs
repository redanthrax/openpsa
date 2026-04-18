using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Configuration;

public class TicketQueueMemberConfiguration : IEntityTypeConfiguration<TicketQueueMember> {
    public void Configure(EntityTypeBuilder<TicketQueueMember> builder) {
        builder.ToTable("TicketQueueMembers");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.UserId).IsRequired().HasMaxLength(64);
        builder.HasIndex(m => new { m.QueueId, m.UserId }).IsUnique();
    }
}
