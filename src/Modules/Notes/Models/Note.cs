using Common.Domain;

namespace OpenPsa.Modules.Notes.Models;

public class Note : BaseEntity {
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public bool IsInternal { get; set; }
}
