using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Settings.Models;

namespace OpenPsa.Modules.Settings.Features.GetGeneralSettings;

public class GetGeneralSettingsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/settings/general", async (OpenPsaDbContext db, CancellationToken ct) => {
            var settings = await db.Set<GeneralSettings>().FirstOrDefaultAsync(ct)
                ?? new GeneralSettings();

            return Results.Ok(Result.Ok(new GeneralSettingsDto {
                CompanyName = settings.CompanyName,
                CompanyEmail = settings.CompanyEmail,
                CompanyPhone = settings.CompanyPhone,
                CompanyWebsite = settings.CompanyWebsite,
                DefaultCurrency = settings.DefaultCurrency,
                DefaultPaymentTermsDays = settings.DefaultPaymentTermsDays
            }));
        }).RequirePermission("settings.view").WithTags("Settings");
    }
}
