using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Expenses.Models;

namespace OpenPsa.Modules.Expenses.Configuration;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense> {
    public void Configure(EntityTypeBuilder<Expense> builder) {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.TicketId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => e.Category);
    }
}
