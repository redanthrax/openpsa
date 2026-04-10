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

namespace OpenPsa.Modules.Authentication.Features.Roles.UpdateRole;

public class UpdateRoleEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/roles/{id:guid}", async (Guid id, UpdateRoleRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var role = await db.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return Results.NotFound();

            var nameTaken = await db.Set<Role>().AnyAsync(r => r.Name == request.Name && r.Id != id, ct);
            if (nameTaken)
                return Results.Json(Result.Fail<RoleDto>("A role with this name already exists"), statusCode: 409);

            role.Name = request.Name;
            role.Description = request.Description ?? string.Empty;
            role.PermissionKeys = request.Permissions;

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(new RoleDto {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.PermissionKeys
            }));
        }).RequirePermission("roles.update").WithTags("Roles");
    }
}
