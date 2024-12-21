namespace Sandbox.FullStackIdentity.Domain;

public class UnauthorizedRequestError : ApiRequestError, INamedError
{
    public new string Name { get; } = "UnauthorizedRequest";

    public UnauthorizedRequestError(string message, HttpRequestException? requestException = null)
        : base(message, requestException)
    {
        
    }

    public UnauthorizedRequestError(HttpRequestException? requestException)
        : base(requestException)
    {
    }
}
