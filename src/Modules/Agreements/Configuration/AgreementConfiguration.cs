using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Agreements.Models;

namespace OpenPsa.Modules.Agreements.Configuration;

public class AgreementConfiguration : IEntityTypeConfiguration<Agreement> {
    public void Configure(EntityTypeBuilder<Agreement> builder) {
        builder.ToTable("Agreements");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(256);
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.MonthlyAmount).HasPrecision(18, 2);
        builder.Property(a => a.TotalValue).HasPrecision(18, 2);
        builder.Property(a => a.BlockHoursTotal).HasPrecision(18, 2);
        builder.Property(a => a.BlockHoursUsed).HasPrecision(18, 2);
        builder.Property(a => a.HourlyRate).HasPrecision(18, 2);
        builder.HasIndex(a => a.ClientId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.EndDate);
        builder.HasIndex(a => a.SlaPolicyId);
    }
}
