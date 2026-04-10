using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Users.AssignRoles;

public record AssignRolesRequest(List<Guid> RoleIds);

public class AssignRolesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/users/{id:guid}/roles", async (Guid id, AssignRolesRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var user = await db.Set<User>().FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null) return Results.NotFound();

            var validRoleIds = await db.Set<Role>()
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(ct);

            user.RoleIds = validRoleIds;
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("users.update").WithTags("Users");
    }
}
