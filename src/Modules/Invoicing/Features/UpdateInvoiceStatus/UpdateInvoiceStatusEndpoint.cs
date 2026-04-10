using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Invoicing;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Invoicing.Models;

namespace OpenPsa.Modules.Invoicing.Features.UpdateInvoiceStatus;

public record UpdateInvoiceStatusRequest(InvoiceStatus Status, decimal? AmountPaid);

public class UpdateInvoiceStatusEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPatch("/api/invoices/{id:guid}/status", async (Guid id, UpdateInvoiceStatusRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var invoice = await db.Set<Invoice>().FirstOrDefaultAsync(i => i.Id == id, ct);
            if (invoice == null) return Results.NotFound();

            invoice.Status = request.Status;
            if (request.AmountPaid.HasValue) invoice.AmountPaid = request.AmountPaid.Value;

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("invoices.update").WithTags("Invoicing");
    }
}
