using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Clients;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.GetClientById;

public class GetClientByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/clients/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var c = await db.Set<Client>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c == null) return Results.NotFound();

            return Results.Ok(Result.Ok(new ClientDto {
                Id = c.Id,
                Name = c.Name,
                Website = c.Website,
                Phone = c.Phone,
                Email = c.Email,
                Notes = c.Notes,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }));
        }).RequirePermission("clients.view").WithTags("Clients");
    }
}
