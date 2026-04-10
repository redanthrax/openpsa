using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Invoicing.Models;

namespace OpenPsa.Modules.Invoicing.Features.DeleteInvoice;

public class DeleteInvoiceEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/invoices/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var invoice = await db.Set<Invoice>().FirstOrDefaultAsync(i => i.Id == id, ct);
            if (invoice == null) return Results.NotFound();

            db.Set<Invoice>().Remove(invoice);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("invoices.delete").WithTags("Invoicing");
    }
}
