using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.Infrastructure;

public class EmptyEmailCreator : IEmailCreator
{

    #region Implementation

    public Task CreateAsync(string localPart, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string localPart, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion
}
