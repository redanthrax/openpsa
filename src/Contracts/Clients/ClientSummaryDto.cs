namespace Contracts.Clients;

public class ClientSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public int ActiveProjects { get; set; }
    public int OpenTickets { get; set; }
}
