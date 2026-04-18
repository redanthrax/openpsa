using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Configuration;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy> {
    public void Configure(EntityTypeBuilder<SlaPolicy> builder) {
        builder.ToTable("SlaPolicies");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.HasIndex(p => p.IsDefault);
        builder.HasMany(p => p.Targets).WithOne().HasForeignKey(t => t.SlaPolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}
