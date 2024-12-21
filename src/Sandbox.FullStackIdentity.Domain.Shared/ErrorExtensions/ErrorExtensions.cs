using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public static class ErrorExtensions
{
    public static string GetName(this IError error)
    {
        return error switch
        {
            INamedError namedError => namedError.Name,
            _ => error.GetType().Name
        };
    }
}
