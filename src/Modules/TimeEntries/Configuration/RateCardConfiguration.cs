using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.TimeEntries.Models;

namespace OpenPsa.Modules.TimeEntries.Configuration;

public class RateCardConfiguration : IEntityTypeConfiguration<RateCard> {
    public void Configure(EntityTypeBuilder<RateCard> builder) {
        builder.ToTable("RateCards");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.ClientId);
        builder.HasIndex(r => r.IsDefault);
        builder.HasMany(r => r.Entries).WithOne().HasForeignKey(e => e.RateCardId).OnDelete(DeleteBehavior.Cascade);
    }
}
