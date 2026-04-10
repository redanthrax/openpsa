using System.Reflection;
using Common.Authorization;
using Common.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Modules;

public interface IModule {
    void RegisterPermissions(IPermissionRegistry registry) { }
    void MapEndpoints(IEndpointRouteBuilder app) {
        var assembly = GetType().Assembly;
        var featureTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpointFeature)) && t is { IsInterface: false, IsAbstract: false });

        foreach (var type in featureTypes) {
            var impl = type.GetMethod(
                nameof(IEndpointFeature.MapEndpoint),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            impl?.Invoke(null, [app]);
        }
    }

    void ConfigureDatabase(ModelBuilder modelBuilder, IPiiEncryptionService? piiEncryption = null) =>
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

    void ConfigureServices(IServiceCollection services) { }
    void ConfigureMiddleware(IApplicationBuilder app) { }
}
