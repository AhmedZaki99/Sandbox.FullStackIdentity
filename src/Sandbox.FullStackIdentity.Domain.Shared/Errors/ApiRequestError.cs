using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class ApiRequestError(string message, HttpRequestException? requestException = null) : Error(message), INamedError
{
    public string Name { get; } = "ApiRequestError";

    public HttpRequestException? RequestException { get; set; } = requestException;

    public ApiRequestError(HttpRequestException? requestException)
        : this(requestException?.Message ?? requestException?.StatusCode?.ToString() ?? "Unknown error", requestException)
    {
    }

    public override string ToString()
    {
        return new ReasonStringBuilder()
            .WithReasonType(GetType())
            .WithInfo(nameof(Message), Message)
            .WithInfo(nameof(RequestException), RequestException?.ToString())
            .WithInfo(nameof(Metadata), string.Join("; ", Metadata))
            .WithInfo(nameof(Reasons), string.Join("; ", Reasons))
            .Build();
    }
}
