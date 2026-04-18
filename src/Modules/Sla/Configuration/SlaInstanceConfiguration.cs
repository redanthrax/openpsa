using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class SlaInstanceConfiguration : IEntityTypeConfiguration<SlaInstance> {
    public void Configure(EntityTypeBuilder<SlaInstance> builder) {
        builder.ToTable("SlaInstances");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.TicketId).IsUnique();
        builder.HasIndex(i => i.SlaPolicyId);
        builder.HasIndex(i => i.ResponseDueAt);
        builder.HasIndex(i => i.ResolutionDueAt);
        builder.HasIndex(i => new { i.ResponseBreached, i.ResolutionBreached });
    }
}
