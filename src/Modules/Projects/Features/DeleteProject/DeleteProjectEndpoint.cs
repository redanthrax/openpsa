using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Projects.Models;

namespace OpenPsa.Modules.Projects.Features.DeleteProject;

public class DeleteProjectEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/projects/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var project = await db.Set<Project>().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project == null) return Results.NotFound();

            db.Set<Project>().Remove(project);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("projects.delete").WithTags("Projects");
    }
}
