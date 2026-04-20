namespace Contracts.Notes;

public class CreateNoteRequest {
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
