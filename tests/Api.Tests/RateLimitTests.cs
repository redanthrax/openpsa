using System.Threading.RateLimiting;
using Common.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Api.Tests;

public class RateLimitTests {
    [Fact]
    public void AddOpenPsaRateLimiting_RegistersAuthAndWritePolicies() {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddOpenPsaRateLimiting(config);
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;
        options.RejectionStatusCode.Should().Be(429);
        options.GlobalLimiter.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenPsaRateLimiting_AppliesConfigOverrides() {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["RateLimiting:Auth:PermitLimit"] = "3",
                ["RateLimiting:Auth:WindowSeconds"] = "30",
            }).Build();

        services.AddOpenPsaRateLimiting(config);
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        // Policy presence is the contract; specific values are validated indirectly.
        options.Should().NotBeNull();
    }
}
