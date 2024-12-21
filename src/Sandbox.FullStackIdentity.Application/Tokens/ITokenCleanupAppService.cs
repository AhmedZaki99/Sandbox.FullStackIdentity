namespace Sandbox.FullStackIdentity.Application;

/// <summary>
/// Provides functionalities for scheduling or enqueuing cleanup processes of expired refresh tokens.
/// </summary>
public interface ITokenCleanupAppService
{
    Task<bool> ScheduleCleanupProcessAsync(string? cronExpression = null, CancellationToken cancellationToken = default);
    Task<bool> EnqueueCleanupProcessAsync(CancellationToken cancellationToken = default);
    Task<bool> CancelCleanupProcessAsync(CancellationToken cancellationToken = default);
}
