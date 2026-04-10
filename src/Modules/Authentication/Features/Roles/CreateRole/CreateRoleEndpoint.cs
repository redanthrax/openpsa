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

namespace OpenPsa.Modules.Authentication.Features.Roles.CreateRole;

public record CreateRoleRequest(string Name, string? Description, List<string> Permissions);

public class CreateRoleEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/roles", async (CreateRoleRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var exists = await db.Set<Role>().AnyAsync(r => r.Name == request.Name, ct);
            if (exists)
                return Results.Json(Result.Fail<RoleDto>("A role with this name already exists"), statusCode: 409);

            var role = new Role {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                PermissionKeys = request.Permissions
            };

            db.Set<Role>().Add(role);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/roles/{role.Id}", Result.Ok(new RoleDto {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.PermissionKeys
            }));
        }).RequirePermission("roles.create").WithTags("Roles");
    }
}
