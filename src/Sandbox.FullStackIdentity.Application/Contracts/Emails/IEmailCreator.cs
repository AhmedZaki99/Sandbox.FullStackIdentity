namespace Sandbox.FullStackIdentity.Application;

public interface IEmailCreator
{
    Task CreateAsync(string localPart, CancellationToken cancellationToken = default);
    Task DeleteAsync(string localPart, CancellationToken cancellationToken = default);
}
