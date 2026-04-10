using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Invoicing;

public class InvoicingModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("invoices", "Invoices", "Invoicing");
    }
}
