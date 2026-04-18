using Common.Database;
using Contracts.Sla;
using Contracts.Tickets;
using IntegrationEvents.Agreements;
using IntegrationEvents.Sla;
using IntegrationEvents.Tickets;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;
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

        var policyResponse = await bus.InvokeAsync<GetSlaPolicyResponse>(new GetSlaPolicyQuery(policyId.Value), ct);
        if (!policyResponse.Found || policyResponse.Targets == null) return;

        var priorityInt = (int)SlaPriorityLevel.Medium;
        if (!policyResponse.Targets.TryGetValue(priorityInt, out var target)) return;

        var now = DateTime.UtcNow;
        var instance = new SlaInstance {
            TicketId = evt.TicketId,
            SlaPolicyId = policyId.Value,
            Priority = (SlaPriorityLevel)priorityInt,
            ResponseDueAt = now.AddMinutes(target.ResponseTimeMinutes),
            ResolutionDueAt = now.AddMinutes(target.ResolutionTimeMinutes)
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
