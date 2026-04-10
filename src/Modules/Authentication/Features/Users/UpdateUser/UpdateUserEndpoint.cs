using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Users.UpdateUser;

public record UpdateUserRequest(string Name, string Email, bool IsActive, bool IsSuperAdmin);

public class UpdateUserEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/users/{id:guid}", async (Guid id, UpdateUserRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var user = await db.Set<User>().FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null) return Results.NotFound();

            var emailTaken = await db.Set<User>().AnyAsync(u => u.Email == request.Email && u.Id != id, ct);
            if (emailTaken)
                return Results.Json(Result.Fail<UserDto>("Email is already in use"), statusCode: 409);

            user.Name = request.Name;
            user.Email = request.Email;
            user.IsActive = request.IsActive;
            user.IsSuperAdmin = request.IsSuperAdmin;

            await db.SaveChangesAsync(ct);

            var roles = await db.Set<Role>()
                .Where(r => user.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);

            return Results.Ok(Result.Ok(new UserDto {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = roles
            }));
        }).RequirePermission("users.update").WithTags("Users");
    }
}
