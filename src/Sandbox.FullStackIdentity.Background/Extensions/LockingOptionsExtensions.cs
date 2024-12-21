using RedLockNet;

namespace Sandbox.FullStackIdentity.Background;

/// <summary>
/// Extension methods for building resources for <see cref="LockingOptions" />.
/// </summary>
public static class LockingOptionsExtensions
{
    public static string GetCleanupGlobalLock(this LockingOptions options)
    {
        return LockingOptions.BuildLockingResource(options.GlobalLockPrefix, options.CleanupLockKey);
    }

    
    public static Task<IRedLock> CreateLockWithDefaultsAsync(this IDistributedLockFactory lockFactory, string resource, LockingOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);
        ArgumentNullException.ThrowIfNull(options);

        return lockFactory.CreateLockAsync(resource, options.LockExpiry, options.DefaultWaitTime, options.DefaultRetryTime, cancellationToken);
    }
}

