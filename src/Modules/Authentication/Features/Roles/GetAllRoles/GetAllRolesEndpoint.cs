using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Roles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Roles.GetAllRoles;

public class GetAllRolesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/roles", async (OpenPsaDbContext db, CancellationToken ct) => {
            var roles = await db.Set<Role>().OrderBy(r => r.Name).ToListAsync(ct);
            var dtos = roles.Select(r => new RoleDto {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.PermissionKeys
            });
            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("roles.list").WithTags("Roles");
    }
}
