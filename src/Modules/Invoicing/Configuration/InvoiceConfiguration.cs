using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Invoicing.Models;

namespace OpenPsa.Modules.Invoicing.Configuration;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice> {
    public void Configure(EntityTypeBuilder<Invoice> builder) {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(64);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.TaxRate).HasPrecision(5, 2);
        builder.Property(i => i.AmountPaid).HasPrecision(18, 2);
        builder.Ignore(i => i.Subtotal);
        builder.Ignore(i => i.TaxAmount);
        builder.Ignore(i => i.Total);
        builder.Ignore(i => i.AmountDue);
        builder.HasMany(i => i.LineItems).WithOne().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => i.ClientId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
    }
}
