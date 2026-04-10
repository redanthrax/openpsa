using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Permissions;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Permissions.GetAllPermissions;

public class GetAllPermissionsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/permissions", async (OpenPsaDbContext db, CancellationToken ct) => {
            var permissions = await db.Set<Permission>()
                .OrderBy(p => p.Category).ThenBy(p => p.Name)
                .Select(p => new PermissionDto {
                    Key = p.Key,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category
                })
                .ToListAsync(ct);

            return Results.Ok(Result.Ok(permissions));
        }).RequirePermission("permissions.list").WithTags("Permissions");
    }
}
