using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class ConflictError(string message) : Error(message), INamedError
{
    public string Name { get; } = "Conflict";
}
