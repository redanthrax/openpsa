using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Contacts;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Contacts.Models;
using Wolverine;

namespace OpenPsa.Modules.Contacts.Features.CreateContact;

public class CreateContactEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/contacts", async (CreateContactRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<ContactDto>("Client not found"), statusCode: 404);

            var contact = new Contact {
                ClientId = request.ClientId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Title = request.Title,
                Email = request.Email,
                Phone = request.Phone,
                IsPrimary = request.IsPrimary
            };

            db.Set<Contact>().Add(contact);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/contacts/{contact.Id}", Result.Ok(new ContactDto {
                Id = contact.Id,
                ClientId = contact.ClientId,
                ClientName = clientResponse.Name ?? string.Empty,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Title = contact.Title,
                Email = contact.Email,
                Phone = contact.Phone,
                IsPrimary = contact.IsPrimary,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            }));
        }).RequirePermission("contacts.create").WithTags("Contacts");
    }
}
