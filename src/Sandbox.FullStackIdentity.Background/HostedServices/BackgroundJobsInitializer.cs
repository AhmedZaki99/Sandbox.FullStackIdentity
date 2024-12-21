using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.Background;

internal class BackgroundJobsInitializer : IHostedService
{

    #region Dependencies

    private readonly IServiceProvider _serviceProvider;

    public BackgroundJobsInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #endregion


    #region Hosted Work

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var tokenCleanupAppService = scope.ServiceProvider.GetRequiredService<ITokenCleanupAppService>();

        await tokenCleanupAppService.ScheduleCleanupProcessAsync(cancellationToken: cancellationToken);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion

}
