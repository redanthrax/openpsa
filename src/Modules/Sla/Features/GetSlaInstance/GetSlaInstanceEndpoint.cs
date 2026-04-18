using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.GetSlaInstance;

public class GetSlaInstanceEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/tickets/{ticketId:guid}/sla", async (Guid ticketId, OpenPsaDbContext db, CancellationToken ct) => {
            var instance = await db.Set<SlaInstance>().FirstOrDefaultAsync(i => i.TicketId == ticketId, ct);
            if (instance == null) return Results.NotFound();

            var policyName = await db.Set<SlaPolicy>().Where(p => p.Id == instance.SlaPolicyId).Select(p => p.Name).FirstOrDefaultAsync(ct);

            var now = DateTime.UtcNow;
            return Results.Ok(Result.Ok(new SlaInstanceDto {
                Id = instance.Id,
                TicketId = instance.TicketId,
                SlaPolicyId = instance.SlaPolicyId,
                SlaPolicyName = policyName ?? string.Empty,
                Priority = instance.Priority,
                ResponseDueAt = instance.ResponseDueAt,
                ResolutionDueAt = instance.ResolutionDueAt,
                RespondedAt = instance.RespondedAt,
                ResolvedAt = instance.ResolvedAt,
                ResponseBreached = instance.ResponseBreached,
                ResolutionBreached = instance.ResolutionBreached,
                ResponseStatus = ComputeStatus(instance.RespondedAt, instance.ResponseDueAt, instance.ResponseBreached, now),
                ResolutionStatus = ComputeStatus(instance.ResolvedAt, instance.ResolutionDueAt, instance.ResolutionBreached, now),
                IsPaused = instance.IsPaused,
                PausedMinutes = instance.PausedMinutes
            }));
        }).RequirePermission("sla.view-instances").WithTags("SLA");
    }

    internal static SlaStatus ComputeStatus(DateTime? completedAt, DateTime? dueAt, bool breached, DateTime now) {
        if (breached) return SlaStatus.Breached;
        if (completedAt.HasValue) return SlaStatus.Healthy;
        if (!dueAt.HasValue) return SlaStatus.Healthy;
        var remaining = dueAt.Value - now;
        if (remaining <= TimeSpan.Zero) return SlaStatus.Breached;
        if (remaining <= TimeSpan.FromMinutes(30)) return SlaStatus.Warning;
        return SlaStatus.Healthy;
    }
}
