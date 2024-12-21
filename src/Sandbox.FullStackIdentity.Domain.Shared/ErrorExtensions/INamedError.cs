using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public interface INamedError : IError
{
    string Name { get; }
}
