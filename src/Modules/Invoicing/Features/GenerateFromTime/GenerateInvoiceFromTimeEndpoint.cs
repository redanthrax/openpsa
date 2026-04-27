using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Invoicing;
using Contracts.Results;
using IntegrationEvents.Clients;
using IntegrationEvents.TimeEntries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Invoicing.Models;
using Wolverine;

namespace OpenPsa.Modules.Invoicing.Features.GenerateFromTime;

public class GenerateInvoiceFromTimeEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/invoices/generate-from-time", async (
            GenerateInvoiceFromTimeRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

                var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
                if (!clientResponse.Found)
                    return Results.Json(Result.Fail<InvoiceDto>("Client not found"), statusCode: 404);

                var timeResponse = await bus.InvokeAsync<GetBillableTimeEntriesForClientResponse>(
                    new GetBillableTimeEntriesForClientQuery(request.ClientId, request.FromDate, request.ToDate), ct);

                if (timeResponse.Entries.Count == 0)
                    return Results.Json(Result.Fail<InvoiceDto>("No billable time entries found for this client"), statusCode: 400);

                var hourlyRate = request.DefaultHourlyRate ?? 150m;

                var lineItems = timeResponse.Entries.Select(e => {
                    var desc = BuildDescription(e);
                    return new InvoiceLineItem {
                        Description = desc,
                        Quantity = e.Hours,
                        UnitPrice = hourlyRate
                    };
                }).ToList();

                var invoiceCount = await db.Set<Invoice>().CountAsync(ct);
                var invoice = new Invoice {
                    InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(invoiceCount + 1):D4}",
                    ClientId = request.ClientId,
                    InvoiceDate = DateTime.UtcNow.Date,
                    DueDate = DateTime.UtcNow.Date.AddDays(request.PaymentTermsDays),
                    LineItems = lineItems
                };

                db.Set<Invoice>().Add(invoice);
                await db.SaveChangesAsync(ct);

                var timeEntryIds = timeResponse.Entries.Select(e => e.TimeEntryId).ToList();
                await bus.InvokeAsync(new MarkTimeEntriesInvoicedCommand(timeEntryIds), ct);

                return Results.Created($"/api/invoices/{invoice.Id}", Result.Ok(new InvoiceDto {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    ClientId = invoice.ClientId,
                    ClientName = clientResponse.Name ?? string.Empty,
                    Status = invoice.Status,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    Subtotal = invoice.Subtotal,
                    TaxAmount = invoice.TaxAmount,
                    Total = invoice.Total,
                    AmountPaid = invoice.AmountPaid,
                    AmountDue = invoice.AmountDue,
                    CreatedAt = invoice.CreatedAt,
                    UpdatedAt = invoice.UpdatedAt,
                    LineItems = invoice.LineItems.Select(l => new InvoiceLineItemDto {
                        Id = l.Id,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        Amount = l.Amount
                    }).ToList()
                }));
            }).RequirePermission("invoices.create").WithTags("Invoicing");
    }

    private static string BuildDescription(BillableTimeEntryData entry) {
        var parts = new List<string>();
        parts.Add(entry.Date.ToString("MMM d"));
        if (!string.IsNullOrEmpty(entry.UserName)) parts.Add(entry.UserName);
        if (!string.IsNullOrEmpty(entry.ProjectName)) parts.Add(entry.ProjectName);
        if (!string.IsNullOrEmpty(entry.TicketTitle)) parts.Add($"#{entry.TicketTitle}");
        if (!string.IsNullOrEmpty(entry.Description)) parts.Add(entry.Description);
        return string.Join(" — ", parts);
    }
}
