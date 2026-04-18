using Common.Domain;
using Contracts.Sla;

namespace OpenPsa.Modules.Sla.Models;

public class SlaTarget : BaseEntity {
    public Guid SlaPolicyId { get; set; }
    public SlaPriorityLevel Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
}
