namespace Contracts.Dashboard;

public class DashboardStatsDto {
    public int TotalClients { get; set; }
    public int ActiveProjects { get; set; }
    public int OpenTickets { get; set; }
    public int OverdueTickets { get; set; }
    public decimal UnbilledHours { get; set; }
    public decimal OutstandingInvoices { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = [];
}
