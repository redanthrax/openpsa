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

namespace OpenPsa.Modules.Settings.Features.UpdateGeneralSettings;

public class UpdateGeneralSettingsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/settings/general", async (GeneralSettingsDto request, OpenPsaDbContext db, CancellationToken ct) => {
            var settings = await db.Set<GeneralSettings>().FirstOrDefaultAsync(ct);
            if (settings == null) {
                settings = new GeneralSettings();
                db.Set<GeneralSettings>().Add(settings);
            }

            settings.CompanyName = request.CompanyName;
            settings.CompanyEmail = request.CompanyEmail;
            settings.CompanyPhone = request.CompanyPhone;
            settings.CompanyWebsite = request.CompanyWebsite;
            settings.DefaultCurrency = request.DefaultCurrency ?? "USD";
            settings.DefaultPaymentTermsDays = request.DefaultPaymentTermsDays;

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(request));
        }).RequirePermission("settings.update").WithTags("Settings");
    }
}
