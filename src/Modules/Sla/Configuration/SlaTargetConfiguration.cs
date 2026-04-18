using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class SlaTargetConfiguration : IEntityTypeConfiguration<SlaTarget> {
    public void Configure(EntityTypeBuilder<SlaTarget> builder) {
        builder.ToTable("SlaTargets");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.SlaPolicyId, t.Priority }).IsUnique();
    }
}
