using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Caching;

public static class CacheExtensions {
    internal const string InstanceName = "OpenPsa:";

    public static IConnectionMultiplexer AddRedisCache(this IServiceCollection services,
            IConfiguration configuration, string? redisConnectionString = null) {
        ArgumentNullException.ThrowIfNull(configuration);
        redisConnectionString ??= configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis connection string not found");

        var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        services.AddStackExchangeRedisCache(options => {
            options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
            options.InstanceName = InstanceName;
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        return multiplexer;
    }
}
