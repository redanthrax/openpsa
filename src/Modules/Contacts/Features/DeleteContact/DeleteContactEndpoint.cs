using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Contacts.Models;

namespace OpenPsa.Modules.Contacts.Features.DeleteContact;

public class DeleteContactEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/contacts/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var contact = await db.Set<Contact>().FirstOrDefaultAsync(c => c.Id == id, ct);
            if (contact == null) return Results.NotFound();

            db.Set<Contact>().Remove(contact);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("contacts.delete").WithTags("Contacts");
    }
}
