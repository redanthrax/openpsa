using Common.Database;
using IntegrationEvents.Authentication;
using IntegrationEvents.Projects;
using IntegrationEvents.Tickets;
using IntegrationEvents.TimeEntries;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Models;
using Wolverine;

namespace OpenPsa.Modules.TimeEntries.Features.Integration;

public class TimeEntryBillingQueryHandler {
    public static async Task<GetBillableTimeEntriesForClientResponse> Handle(
        GetBillableTimeEntriesForClientQuery query, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) {

        var q = db.Set<TimeEntry>()
            .Where(t => t.ClientId == query.ClientId && t.Billable && !t.Invoiced);

        if (query.FromDate.HasValue) q = q.Where(t => t.Date >= query.FromDate.Value);
        if (query.ToDate.HasValue) q = q.Where(t => t.Date <= query.ToDate.Value);

        var entries = await q.OrderBy(t => t.Date).ToListAsync(ct);
        if (entries.Count == 0)
            return new GetBillableTimeEntriesForClientResponse([]);

        var projIds = entries.Where(t => t.ProjectId.HasValue).Select(t => t.ProjectId!.Value).Distinct().ToList();
        var projectNames = projIds.Count > 0
            ? (await bus.InvokeAsync<GetProjectNamesResponse>(new GetProjectNamesQuery(projIds), ct)).Names
            : new Dictionary<Guid, string>();

        var ticketIds = entries.Where(t => t.TicketId.HasValue).Select(t => t.TicketId!.Value).Distinct().ToList();
        var ticketTitles = ticketIds.Count > 0
            ? (await bus.InvokeAsync<GetTicketTitlesResponse>(new GetTicketTitlesQuery(ticketIds), ct)).Titles
            : new Dictionary<Guid, string>();

        var userIds = entries.Select(t => t.UserId).Distinct().ToList();
        var userNames = (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(userIds), ct)).Names;

        var data = entries.Select(t => new BillableTimeEntryData(
            t.Id,
            t.ClientId,
            t.ProjectId,
            t.ProjectId.HasValue ? projectNames.GetValueOrDefault(t.ProjectId.Value) : null,
            t.TicketId,
            t.TicketId.HasValue ? ticketTitles.GetValueOrDefault(t.TicketId.Value) : null,
            userNames.GetValueOrDefault(t.UserId, string.Empty),
            t.Date,
            t.Hours,
            t.Description
        )).ToList();

        return new GetBillableTimeEntriesForClientResponse(data);
    }

    public static async Task Handle(MarkTimeEntriesInvoicedCommand command, OpenPsaDbContext db, CancellationToken ct) {
        await db.Set<TimeEntry>()
            .Where(t => command.TimeEntryIds.Contains(t.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Invoiced, true), ct);
    }
}
