using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class AccessDeniedError(string message) : Error(message), INamedError
{
    public string Name { get; } = "AccessDenied";
}
