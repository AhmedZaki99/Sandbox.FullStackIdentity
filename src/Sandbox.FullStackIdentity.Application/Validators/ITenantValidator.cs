using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface ITenantValidator
{
    Task<Result> ValidateAsync(Tenant tenant, CancellationToken cancellationToken);
}
