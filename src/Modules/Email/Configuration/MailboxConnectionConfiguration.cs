using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Configuration;

public class MailboxConnectionConfiguration : IEntityTypeConfiguration<MailboxConnection> {
    public void Configure(EntityTypeBuilder<MailboxConnection> builder) {
        builder.ToTable("MailboxConnections");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(200);
        builder.Property(m => m.EmailAddress).HasMaxLength(500);
        builder.Property(m => m.ImapHost).HasMaxLength(500);
        builder.Property(m => m.SmtpHost).HasMaxLength(500);
        builder.Property(m => m.ImapUsername).HasMaxLength(500);
        builder.Property(m => m.SmtpUsername).HasMaxLength(500);
        builder.Property(m => m.GraphTenantId).HasMaxLength(200);
        builder.Property(m => m.GraphClientId).HasMaxLength(200);
        builder.Property(m => m.GraphMailboxUserId).HasMaxLength(500);
        builder.HasIndex(m => m.EmailAddress).IsUnique();
        builder.HasIndex(m => m.Status);
        builder.HasMany(m => m.Messages)
            .WithOne(e => e.MailboxConnection)
            .HasForeignKey(e => e.MailboxConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
