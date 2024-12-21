using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class NotFoundError(string message) : Error(message), INamedError
{
    public string Name { get; } = "NotFound";
}
