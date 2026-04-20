using Common.Database;
using Contracts.Sla;
using Contracts.Tickets;
using IntegrationEvents.Agreements;
using IntegrationEvents.Sla;
using IntegrationEvents.Tickets;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;
using OpenPsa.Modules.Sla.Services;
using Wolverine;

namespace OpenPsa.Modules.Sla.Features.Integration;

public class SlaTicketEventHandler {
    public static async Task Handle(TicketCreated evt, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) {
        var agreementResponse = await bus.InvokeAsync<GetActiveAgreementForClientResponse>(
            new GetActiveAgreementForClientQuery(evt.ClientId), ct);

        Guid? policyId = agreementResponse.SlaPolicyId;

        if (!policyId.HasValue) {
            var defaultPolicy = await bus.InvokeAsync<GetDefaultSlaPolicyResponse>(new GetDefaultSlaPolicyQuery(), ct);
            policyId = defaultPolicy.PolicyId;
        }

        if (!policyId.HasValue) return;

        var policy = await db.Set<SlaPolicy>().FirstOrDefaultAsync(p => p.Id == policyId.Value, ct);
        if (policy is null) return;

        var policyResponse = await bus.InvokeAsync<GetSlaPolicyResponse>(new GetSlaPolicyQuery(policyId.Value), ct);
        if (!policyResponse.Found || policyResponse.Targets == null) return;

        var priorityInt = (int)SlaPriorityLevel.Medium;
        if (!policyResponse.Targets.TryGetValue(priorityInt, out var target)) return;

        BusinessHoursCalendar? calendar = null;
        if (policy.BusinessHoursCalendarId.HasValue) {
            calendar = await db.Set<BusinessHoursCalendar>()
                .Include(c => c.Schedules)
                .Include(c => c.Holidays)
                .FirstOrDefaultAsync(c => c.Id == policy.BusinessHoursCalendarId.Value, ct);
        }

        var now = DateTime.UtcNow;
        var instance = new SlaInstance {
            TicketId = evt.TicketId,
            SlaPolicyId = policyId.Value,
            Priority = (SlaPriorityLevel)priorityInt,
            ResponseDueAt = BusinessHoursService.CalculateDeadline(now, target.ResponseTimeMinutes, calendar),
            ResolutionDueAt = BusinessHoursService.CalculateDeadline(now, target.ResolutionTimeMinutes, calendar)
        };

        db.Set<SlaInstance>().Add(instance);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Handle(TicketUpdated evt, OpenPsaDbContext db, CancellationToken ct) {
        var instance = await db.Set<SlaInstance>().FirstOrDefaultAsync(i => i.TicketId == evt.TicketId, ct);
        if (instance == null) return;

        await db.SaveChangesAsync(ct);
    }
}
