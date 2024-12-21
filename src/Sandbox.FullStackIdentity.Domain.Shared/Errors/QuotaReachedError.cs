using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class QuotaReachedError(string message) : Error(message), INamedError
{
    public string Name { get; } = "QuotaReached";
}
