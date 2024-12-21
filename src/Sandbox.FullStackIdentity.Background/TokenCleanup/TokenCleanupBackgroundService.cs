using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Background;

/// <inheritdoc/>
internal sealed class TokenCleanupBackgroundService : ITokenCleanupBackgroundService
{

    #region Dependencies

    private readonly IDistributedLockFactory _distributedLockFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOptions<LockingOptions> _lockingOptions;
    private readonly ILogger<TokenCleanupBackgroundService> _logger;

    public TokenCleanupBackgroundService(
        IDistributedLockFactory distributedLockFactory,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<LockingOptions> lockingOptionsAccessor,
        ILogger<TokenCleanupBackgroundService> logger)
    {
        _distributedLockFactory = distributedLockFactory;
        _refreshTokenRepository = refreshTokenRepository;
        _lockingOptions = lockingOptionsAccessor;
        _logger = logger;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Result> InitiateCleanupProcessAsync(CancellationToken cancellationToken = default)
    {
        // Prevent processing conflicts.
        var lockingResource = _lockingOptions.Value.GetCleanupGlobalLock();

        await using var redLock = await _distributedLockFactory.CreateLockWithDefaultsAsync(lockingResource, _lockingOptions.Value, cancellationToken);
        if (!redLock.IsAcquired)
        {
            _logger.LogDebug("Cleanup process has been skipped due to concurrency conflicts.");
            return new ConflictError("The process failed to execute due to concurrent usage of shared resources.");
        }

        var deletedCount = await _refreshTokenRepository.DeleteExpiredAsync(cancellationToken);
        if (deletedCount > 0)
        {
            _logger.LogInformation("{count} expired refresh tokens were deleted in the background.", deletedCount);
        }

        return Result.Ok();
    }
    
    #endregion

}
