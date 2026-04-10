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

namespace OpenPsa.Modules.Clients.Features.GetAllClients;

public class GetAllClientsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/clients", async (OpenPsaDbContext db, CancellationToken ct) => {
            var clients = await db.Set<Client>()
                .OrderBy(c => c.Name)
                .Select(c => new ClientDto {
                    Id = c.Id,
                    Name = c.Name,
                    Website = c.Website,
                    Phone = c.Phone,
                    Email = c.Email,
                    Notes = c.Notes,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(ct);

            return Results.Ok(Result.Ok(clients));
        }).RequirePermission("clients.list").WithTags("Clients");
    }
}
