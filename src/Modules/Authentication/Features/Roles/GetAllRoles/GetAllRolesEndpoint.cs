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
        app.MapGet("/api/roles", async (
            OpenPsaDbContext db,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

            var ordered = db.Set<Role>().OrderBy(r => r.Name);
            var totalCount = await ordered.CountAsync(ct);
            var roles = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var dtos = roles.Select(r => new RoleDto {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.PermissionKeys
            }).ToList();

            return Results.Ok(PagedResult.Ok<RoleDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("roles.list").WithTags("Roles");
    }
}
