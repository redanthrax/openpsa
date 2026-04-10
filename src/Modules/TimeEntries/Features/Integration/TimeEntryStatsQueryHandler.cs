using Common.Database;
using IntegrationEvents.TimeEntries;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Models;

namespace OpenPsa.Modules.TimeEntries.Features.Integration;

public class TimeEntryStatsQueryHandler {
    private readonly OpenPsaDbContext _db;
    public TimeEntryStatsQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetUnbilledHoursResponse> Handle(GetUnbilledHoursQuery query) {
        var hours = await _db.Set<TimeEntry>()
            .Where(t => t.Billable && !t.Invoiced)
            .SumAsync(t => (decimal?)t.Hours) ?? 0m;
        return new(hours);
    }
}
