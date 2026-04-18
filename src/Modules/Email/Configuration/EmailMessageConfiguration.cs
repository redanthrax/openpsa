using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Configuration;

public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage> {
    public void Configure(EntityTypeBuilder<EmailMessage> builder) {
        builder.ToTable("EmailMessages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FromAddress).HasMaxLength(500);
        builder.Property(e => e.FromName).HasMaxLength(500);
        builder.Property(e => e.ToAddress).HasMaxLength(500);
        builder.Property(e => e.Subject).HasMaxLength(1000);
        builder.Property(e => e.MessageId).HasMaxLength(1000);
        builder.Property(e => e.InReplyTo).HasMaxLength(1000);
        builder.HasIndex(e => e.MailboxConnectionId);
        builder.HasIndex(e => e.TicketId);
        builder.HasIndex(e => e.ContactId);
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.MessageId);
        builder.HasIndex(e => e.Direction);
        builder.HasIndex(e => e.SentAt);
    }
}
