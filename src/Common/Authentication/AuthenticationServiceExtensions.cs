using Microsoft.Extensions.DependencyInjection;

namespace Common.Authentication;

public static class AuthenticationServiceExtensions {
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services) {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }
}
