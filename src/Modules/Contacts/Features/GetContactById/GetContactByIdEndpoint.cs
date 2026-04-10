using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Contacts;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Contacts.Models;
using Wolverine;

namespace OpenPsa.Modules.Contacts.Features.GetContactById;

public class GetContactByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/contacts/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var contact = await db.Set<Contact>().FirstOrDefaultAsync(c => c.Id == id, ct);
            if (contact == null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(contact.ClientId), ct)).Name ?? string.Empty;

            return Results.Ok(Result.Ok(new ContactDto {
                Id = contact.Id,
                ClientId = contact.ClientId,
                ClientName = clientName,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Title = contact.Title,
                Email = contact.Email,
                Phone = contact.Phone,
                IsPrimary = contact.IsPrimary,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            }));
        }).RequirePermission("contacts.view").WithTags("Contacts");
    }
}
