using Common.Database;
using Contracts.Invoicing;
using IntegrationEvents.Invoicing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Invoicing.Models;

namespace OpenPsa.Modules.Invoicing.Features.Integration;

public class InvoicingStatsQueryHandler {
    private readonly OpenPsaDbContext _db;
    public InvoicingStatsQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetOutstandingInvoicesTotalResponse> Handle(GetOutstandingInvoicesTotalQuery query) {
        var outstandingStatuses = new[] { InvoiceStatus.Sent, InvoiceStatus.Overdue, InvoiceStatus.PartiallyPaid };

        var invoices = await _db.Set<Invoice>()
            .Include(i => i.LineItems)
            .Where(i => outstandingStatuses.Contains(i.Status))
            .ToListAsync();

        var total = invoices.Sum(i => i.AmountDue);
        return new(total);
    }
}
