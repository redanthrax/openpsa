using Microsoft.AspNetCore.Builder;

namespace Common.Authentication;

public static class AuthenticationExtensions {
    public static RouteHandlerBuilder RequireRole(this RouteHandlerBuilder builder, params string[] roles) {
        return builder.RequireAuthorization(policy => policy.RequireRole(roles));
    }
}
