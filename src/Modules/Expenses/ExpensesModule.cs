using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Expenses;

public class ExpensesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("expenses", "Expenses", "Expenses");
        // TODO: register "expenses.approve" once an approval endpoint is implemented.
    }
}
