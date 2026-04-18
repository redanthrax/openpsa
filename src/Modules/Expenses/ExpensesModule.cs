using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Expenses;

public class ExpensesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("expenses", "Expenses", "Expenses");
        registry.RegisterPermission("expenses.approve", "Approve Expenses", "Approve or reject submitted expenses", "Expenses");
    }
}
