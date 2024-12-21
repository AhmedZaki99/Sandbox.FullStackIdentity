using FluentResults;

namespace Sandbox.FullStackIdentity.Application;

public interface ITenantValidator
{
    Task<Result> ValidateAsync(string handle, CancellationToken cancellationToken);
}
