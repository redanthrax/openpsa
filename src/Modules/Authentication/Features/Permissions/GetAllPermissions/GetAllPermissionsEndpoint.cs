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
        app.MapGet("/api/permissions", async (
            OpenPsaDbContext db,
            int page = 1, int pageSize = 100,
            CancellationToken ct = default) => {

            var query = db.Set<Permission>()
                .OrderBy(p => p.Category).ThenBy(p => p.Name)
                .Select(p => new PermissionDto {
                    Key = p.Key,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category
                });

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(PagedResult.Ok(items, totalCount, page, pageSize));
        }).RequirePermission("permissions.list").WithTags("Permissions");
    }
}
