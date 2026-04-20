using Common.Authorization;
using Common.Modules;
using QuestPDF.Infrastructure;

namespace OpenPsa.Modules.Invoicing;

public class InvoicingModule : IModule {
    static InvoicingModule() {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("invoices", "Invoices", "Invoicing");
    }
}
