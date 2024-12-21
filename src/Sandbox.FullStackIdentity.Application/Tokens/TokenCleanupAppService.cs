using Hangfire;
using Microsoft.Extensions.Logging;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class TokenCleanupAppService : ITokenCleanupAppService
{

    #region Constants

    private const string TokenCleanupJobId = "TokenCleanupJob.Id";

    #endregion

    #region Dependencies

    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IConfigRepository _configRepository;
    private readonly ILogger<TokenCleanupAppService> _logger;

    public TokenCleanupAppService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IConfigRepository configRepository,
        ILogger<TokenCleanupAppService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _configRepository = configRepository;
        _logger = logger;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<bool> ScheduleCleanupProcessAsync(string? cronExpression = null, CancellationToken cancellationToken = default)
    {
        if (cronExpression is not null)
        {
            await _configRepository.SetAsync(ConfigurationKeys.TokenCleanupJobCron, cronExpression, cancellationToken);
        }
        else
        {
            cronExpression = await _configRepository.GetAsync(ConfigurationKeys.TokenCleanupJobCron, cancellationToken);
            if (cronExpression is null)
            {
                _logger.LogWarning("Failed to schedule the token cleanup job due to missing configuration.");
                return false;
            }
        }
        _recurringJobManager.AddOrUpdate<ITokenCleanupBackgroundService>(TokenCleanupJobId, backgroundService => 
            backgroundService.InitiateCleanupProcessAsync(CancellationToken.None), cronExpression);

        _logger.LogInformation("The token cleanup job has been scheduled to run in the background. Job CRON expression: '{cronExpression}'", cronExpression);
        return true;
    }

    /// <inheritdoc/>
    public Task<bool> EnqueueCleanupProcessAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _backgroundJobClient.Enqueue<ITokenCleanupBackgroundService>(backgroundService =>
            backgroundService.InitiateCleanupProcessAsync(CancellationToken.None));

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<bool> CancelCleanupProcessAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _recurringJobManager.RemoveIfExists(TokenCleanupJobId);

        _logger.LogInformation("The token cleanup job has been canceled from background schedule.");
        return Task.FromResult(true);
    }

    #endregion

}
