using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Contacts.Models;

namespace OpenPsa.Modules.Contacts.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact> {
    public void Configure(EntityTypeBuilder<Contact> builder) {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(128);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Title).HasMaxLength(128);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(64);
        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.Email);
    }
}
