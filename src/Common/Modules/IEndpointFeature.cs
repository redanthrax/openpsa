using Microsoft.AspNetCore.Routing;

namespace Common.Modules;

public interface IEndpointFeature {
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
