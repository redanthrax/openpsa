using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Invoicing.Models;

namespace OpenPsa.Modules.Invoicing.Configuration;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem> {
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder) {
        builder.ToTable("InvoiceLineItems");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(512);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(l => l.Amount);
        builder.HasIndex(l => l.InvoiceId);
    }
}
