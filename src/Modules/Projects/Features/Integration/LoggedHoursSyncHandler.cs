using Common.Database;
using IntegrationEvents.TimeEntries;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Projects.Models;

namespace OpenPsa.Modules.Projects.Features.Integration;

public class LoggedHoursSyncHandler {
    private readonly OpenPsaDbContext _db;
    public LoggedHoursSyncHandler(OpenPsaDbContext db) => _db = db;

    public async Task Handle(TimeEntryLogged @event) {
        if (!@event.ProjectId.HasValue) return;
        await _db.Set<Project>()
            .Where(p => p.Id == @event.ProjectId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LoggedHours, p => p.LoggedHours + @event.Hours));
    }

    public async Task Handle(TimeEntryUpdated @event) {
        if (!@event.ProjectId.HasValue) return;
        await _db.Set<Project>()
            .Where(p => p.Id == @event.ProjectId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LoggedHours, p => p.LoggedHours + @event.Hours));
    }

    public async Task Handle(TimeEntryDeleted @event) {
        if (!@event.ProjectId.HasValue) return;
        await _db.Set<Project>()
            .Where(p => p.Id == @event.ProjectId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LoggedHours, p => p.LoggedHours - @event.Hours));
    }
}
