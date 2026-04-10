using Common.Domain;
using Contracts.Clients;

namespace OpenPsa.Modules.Clients.Models;

public class Client : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
}
