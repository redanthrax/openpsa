using Common.Database;
using IntegrationEvents.Projects;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Projects.Models;

namespace OpenPsa.Modules.Projects.Features.Integration;

public class ProjectNameQueryHandler {
    private readonly OpenPsaDbContext _db;
    public ProjectNameQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetProjectNameResponse> Handle(GetProjectNameQuery query) {
        var name = await _db.Set<Project>().Where(p => p.Id == query.ProjectId)
            .Select(p => p.Name).FirstOrDefaultAsync();
        return name != null ? new(true, name) : new(false, null);
    }

    public async Task<GetProjectNamesResponse> Handle(GetProjectNamesQuery query) {
        var names = await _db.Set<Project>()
            .Where(p => query.ProjectIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);
        return new(names);
    }
}
