using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Projects.Models;

namespace OpenPsa.Modules.Projects.Configuration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project> {
    public void Configure(EntityTypeBuilder<Project> builder) {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.ManagerUserId).HasMaxLength(64);
        builder.Property(p => p.BudgetHours).HasPrecision(18, 2);
        builder.Property(p => p.BudgetAmount).HasPrecision(18, 2);
        builder.Property(p => p.LoggedHours).HasPrecision(18, 2);
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.Status);
    }
}
