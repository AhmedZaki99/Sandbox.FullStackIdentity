using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class BackgroundServiceCollectionExtensions
{

    /// <summary>
    /// Adds RedLock distributed locking services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="configureOptions">A delegate to configure <see cref="LockingOptions"/>.</param>
    /// <returns>A <see cref="BackgroundBuilder"/> that can be used to further configure background services.</returns>
    public static BackgroundBuilder AddRedLock(this AppBuilder builder, IList<RedLockEndPoint> endPoints, Action<LockingOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Add RedLock Factory.
        builder.Services.AddSingleton<IDistributedLockFactory, RedLockFactory>(sp =>
            RedLockFactory.Create(endPoints, sp.GetRedLockRetryConfiguration(), sp.GetService<ILoggerFactory>())
        );

        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }
        return new(builder.Services);
    }

    /// <summary>
    /// Adds background service implementations to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <returns>The <see cref="BackgroundBuilder"/> to allow chaining up service configuration.</returns>
    public static BackgroundBuilder AddBackgroundServices(this BackgroundBuilder builder)
    {
        builder.Services.AddScoped<ITokenCleanupBackgroundService, TokenCleanupBackgroundService>();

        builder.Services.AddHostedService<BackgroundJobsInitializer>();

        return new(builder.Services);
    }


    private static RedLockRetryConfiguration? GetRedLockRetryConfiguration(this IServiceProvider serviceProvider)
    {
        var lockingOptionsAccessor = serviceProvider.GetService<IOptions<LockingOptions>>();
        if (lockingOptionsAccessor?.Value is not LockingOptions options)
        {
            return null;
        }
        if (options.RetryCount is null && options.RetryDelayMs is null)
        {
            return null;
        }
        return new(options.RetryCount, options.RetryDelayMs);
    }

}
