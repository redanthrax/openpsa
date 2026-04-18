using Common.Database;
using Contracts.Sla;
using IntegrationEvents.Sla;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.Integration;

public class SlaPolicyQueryHandler {
    public static async Task<GetSlaPolicyResponse> Handle(GetSlaPolicyQuery query, OpenPsaDbContext db, CancellationToken ct) {
        var policy = await db.Set<SlaPolicy>().Include(p => p.Targets).FirstOrDefaultAsync(p => p.Id == query.SlaPolicyId, ct);
        if (policy == null) return new GetSlaPolicyResponse(false, null, null);

        var targets = policy.Targets.ToDictionary(
            t => (int)t.Priority,
            t => new SlaPolicyTargetData(t.ResponseTimeMinutes, t.ResolutionTimeMinutes));

        return new GetSlaPolicyResponse(true, policy.Name, targets);
    }

    public static async Task<GetDefaultSlaPolicyResponse> Handle(GetDefaultSlaPolicyQuery query, OpenPsaDbContext db, CancellationToken ct) {
        var policy = await db.Set<SlaPolicy>().Where(p => p.IsDefault).Select(p => new { p.Id, p.Name }).FirstOrDefaultAsync(ct);
        return policy != null
            ? new GetDefaultSlaPolicyResponse(policy.Id, policy.Name)
            : new GetDefaultSlaPolicyResponse(null, null);
    }
}
