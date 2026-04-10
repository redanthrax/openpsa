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
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Users.CreateUser;

public record CreateUserRequest(string Email, string Name, string Password, bool IsSuperAdmin = false);

public class CreateUserEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/users", async (CreateUserRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var exists = await db.Set<User>().AnyAsync(u => u.Email == request.Email, ct);
            if (exists)
                return Results.Json(Result.Fail<UserDto>("A user with this email already exists"), statusCode: 409);

            var user = new User {
                Email = request.Email,
                Name = request.Name,
                IsSuperAdmin = request.IsSuperAdmin,
                LocalPasswordHash = PasswordHasher.Hash(request.Password)
            };

            db.Set<User>().Add(user);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/users/{user.Id}", Result.Ok(new UserDto {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = []
            }));
        }).RequirePermission("users.create").WithTags("Users");
    }
}
