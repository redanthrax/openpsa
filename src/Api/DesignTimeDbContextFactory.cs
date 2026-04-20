using Common.Database;
using Common.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OpenPsa.Modules.Agreements;
using OpenPsa.Modules.Assets;
using OpenPsa.Modules.Authentication;
using OpenPsa.Modules.Clients;
using OpenPsa.Modules.Contacts;
using OpenPsa.Modules.Dashboard;
using OpenPsa.Modules.Email;
using OpenPsa.Modules.Expenses;
using OpenPsa.Modules.Invoicing;
using OpenPsa.Modules.Notes;
using OpenPsa.Modules.Projects;
using OpenPsa.Modules.Security;
using OpenPsa.Modules.Settings;
using OpenPsa.Modules.Sla;
using OpenPsa.Modules.Tickets;
using OpenPsa.Modules.TimeEntries;

namespace Api;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OpenPsaDbContext> {
    public OpenPsaDbContext CreateDbContext(string[] args) {
        var builder = new DbContextOptionsBuilder<OpenPsaDbContext>();
        builder.UseNpgsql("Host=localhost;Database=openpsa_design;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly("Api"));

        IModule[] modules = [
            new AuthenticationModule(),
            new ClientsModule(),
            new ContactsModule(),
            new ProjectsModule(),
            new TicketsModule(),
            new TimeEntriesModule(),
            new InvoicingModule(),
            new NotesModule(),
            new SettingsModule(),
            new DashboardModule(),
            new SecurityModule(),
            new AgreementsModule(),
            new AssetsModule(),
            new EmailModule(),
            new ExpensesModule(),
            new SlaModule(),
        ];

        return new OpenPsaDbContext(builder.Options, modules);
    }
}
