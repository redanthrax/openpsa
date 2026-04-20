using Common.Authorization;
using Common.Modules;
using Microsoft.Extensions.DependencyInjection;
using OpenPsa.Modules.Email.Services;
using OpenPsa.Modules.Email.Templates;

namespace OpenPsa.Modules.Email;

public class EmailModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("mailbox-connections", "Mailbox Connections", "Email");
        registry.RegisterPermission("email.send", "Send Email", "Send email from ticket", "Email");
        registry.RegisterPermission("email.view-messages", "View Email Messages", "View email message history", "Email");
    }

    public void ConfigureServices(IServiceCollection services) {
        services.AddScoped<GraphMailService>();
        services.AddScoped<InboundEmailProcessor>();
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        services.AddHostedService<ImapPollingService>();
        services.AddHostedService<GraphPollingService>();
    }
}
