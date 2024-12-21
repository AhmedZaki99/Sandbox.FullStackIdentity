using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class InternalError(string message, Exception? exception = null) : Error(message), INamedError
{
    public string Name { get; } = "InternalError";

    public Exception? Exception { get; set; } = exception;

    public InternalError(Exception exception) : this(exception.Message, exception)
    {
    }

    public override string ToString()
    {
        return new ReasonStringBuilder()
            .WithReasonType(GetType())
            .WithInfo(nameof(Message), Message)
            .WithInfo(nameof(Exception), Exception?.ToString())
            .WithInfo(nameof(Metadata), string.Join("; ", Metadata))
            .WithInfo(nameof(Reasons), string.Join("; ", Reasons))
            .Build();
    }
}
