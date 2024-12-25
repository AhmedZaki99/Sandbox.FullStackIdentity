using Hope.Configuration;

namespace Sandbox.FullStackIdentity.Background;

public sealed class LockingOptions : IKeyedOptions
{
    public const string Key = "Locking";
    string IKeyedOptions.Key => Key;


    public TimeSpan LockExpiry { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DefaultWaitTime { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan DefaultRetryTime { get; set; } = TimeSpan.FromSeconds(1);

    public string GlobalLockPrefix { get; set; } = "global";
    public string ResourceLockPrefix { get; set; } = "resource";

    public string CleanupLockKey { get; set; } = "cleanup";

    public int? RetryCount { get; set; }
    public int? RetryDelayMs { get; set; }


    public static string BuildLockingResource(string lockPrefix, string lockKey, string? lockIdentifier = null)
    {
        return string.IsNullOrWhiteSpace(lockIdentifier)
            ? $"{lockPrefix}:{lockKey}"
            : $"{lockPrefix}:{lockKey}:{lockIdentifier}";
    }
}