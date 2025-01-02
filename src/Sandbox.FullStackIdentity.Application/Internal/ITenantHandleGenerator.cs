namespace Sandbox.FullStackIdentity.Application;

internal interface ITenantHandleGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
