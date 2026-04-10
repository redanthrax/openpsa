using Common.Database;
using Contracts.Projects;
using IntegrationEvents.Projects;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Projects.Models;

namespace OpenPsa.Modules.Projects.Features.Integration;

public class ProjectStatsQueryHandler {
    private readonly OpenPsaDbContext _db;
    public ProjectStatsQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetActiveProjectCountResponse> Handle(GetActiveProjectCountQuery query) {
        var count = await _db.Set<Project>().CountAsync(p => p.Status == ProjectStatus.Active);
        return new(count);
    }
}
