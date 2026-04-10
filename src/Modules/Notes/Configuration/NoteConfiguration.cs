using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPsa.Modules.Notes.Models;

namespace OpenPsa.Modules.Notes.Configuration;

public class NoteConfiguration : IEntityTypeConfiguration<Note> {
    public void Configure(EntityTypeBuilder<Note> builder) {
        builder.ToTable("Notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.EntityType).IsRequired().HasMaxLength(64);
        builder.Property(n => n.Content).IsRequired().HasMaxLength(4000);
        builder.HasIndex(n => new { n.EntityType, n.EntityId });
        builder.HasIndex(n => n.UserId);
    }
}
