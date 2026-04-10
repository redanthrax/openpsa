using System.Reflection;
using Common.Authorization;
using Common.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Modules;

public static class ModuleExtensions {
    public static IServiceCollection AddModules(this IServiceCollection services, Assembly[] assemblies) {
        ArgumentNullException.ThrowIfNull(assemblies);
        var moduleInstances = new List<IModule>();

        foreach (var assembly in assemblies) {
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

            foreach (var moduleType in moduleTypes) {
                var instance = (IModule)Activator.CreateInstance(moduleType)!;
                moduleInstances.Add(instance);
                services.AddSingleton<IModule>(instance);
            }
        }

        foreach (var module in moduleInstances) {
            module.ConfigureServices(services);
        }

        var registryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPermissionRegistry));
        if (registryDescriptor?.ImplementationInstance is IPermissionRegistry registry) {
            foreach (var module in moduleInstances) {
                module.RegisterPermissions(registry);
            }
        }

        return services;
    }

    public static void ConfigureModuleDatabase(this ModelBuilder modelBuilder,
            IEnumerable<IModule> modules, IPiiEncryptionService? piiEncryption = null) {
        ArgumentNullException.ThrowIfNull(modules);
        foreach (var module in modules) {
            module.ConfigureDatabase(modelBuilder, piiEncryption);
        }
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder app) {
        ArgumentNullException.ThrowIfNull(app);
        foreach (var module in app.ServiceProvider.GetServices<IModule>()) {
            module.MapEndpoints(app);
        }
        return app;
    }

    public static IApplicationBuilder UseModules(this IApplicationBuilder app) {
        ArgumentNullException.ThrowIfNull(app);
        foreach (var module in app.ApplicationServices.GetServices<IModule>()) {
            module.ConfigureMiddleware(app);
        }
        return app;
    }
}
