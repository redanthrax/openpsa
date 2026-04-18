using Common.Database;
using Contracts.Agreements;
using IntegrationEvents.Agreements;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Agreements.Models;

namespace OpenPsa.Modules.Agreements.Features.Integration;

public class AgreementQueryHandler {
    public static async Task<GetAgreementNameResponse> Handle(GetAgreementNameQuery query, OpenPsaDbContext db, CancellationToken ct) {
        var name = await db.Set<Agreement>().Where(a => a.Id == query.AgreementId).Select(a => a.Name).FirstOrDefaultAsync(ct);
        return new GetAgreementNameResponse(name != null, name);
    }

    public static async Task<GetAgreementNamesResponse> Handle(GetAgreementNamesQuery query, OpenPsaDbContext db, CancellationToken ct) {
        var names = await db.Set<Agreement>()
            .Where(a => query.AgreementIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, ct);
        return new GetAgreementNamesResponse(names);
    }

    public static async Task<GetActiveAgreementForClientResponse> Handle(GetActiveAgreementForClientQuery query, OpenPsaDbContext db, CancellationToken ct) {
        var agreement = await db.Set<Agreement>()
            .Where(a => a.ClientId == query.ClientId && a.Status == AgreementStatus.Active)
            .OrderByDescending(a => a.StartDate)
            .Select(a => new { a.Id, a.Name, a.SlaPolicyId })
            .FirstOrDefaultAsync(ct);

        return agreement != null
            ? new GetActiveAgreementForClientResponse(agreement.Id, agreement.Name, agreement.SlaPolicyId)
            : new GetActiveAgreementForClientResponse(null, null, null);
    }
}
