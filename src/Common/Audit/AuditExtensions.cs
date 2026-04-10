using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Audit;

public static class AuditExtensions {
    public static IServiceCollection AddAuditTrail(this IServiceCollection services) {
        services.AddSingleton<IAuditConfiguration, DefaultAuditConfiguration>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddHostedService<AuditPurgeBackgroundService>();
        return services;
    }

    public static DbContextOptionsBuilder UseAuditInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider) {
        var interceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
        optionsBuilder.AddInterceptors(interceptor);
        return optionsBuilder;
    }
}
