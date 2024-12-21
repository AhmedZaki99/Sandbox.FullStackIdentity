using FluentResults;

namespace Sandbox.FullStackIdentity.Application;

/// <summary>
/// Provides functionalities for handling cleanup of expired refresh tokens.
/// </summary>
/// <remarks>
/// This service is designed to be called from a background coordinator (e.g., Hangfire).
/// Direct injection of this service into other application services will throw an exception.
/// </remarks>
public interface ITokenCleanupBackgroundService
{
    Task<Result> InitiateCleanupProcessAsync(CancellationToken cancellationToken = default);
}
