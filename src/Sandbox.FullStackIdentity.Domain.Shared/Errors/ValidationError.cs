using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public class ValidationError : Error, INamedError
{
    public string Name { get; } = "ValidationError";

    public string? PropertyName { get; protected set; }

    public ValidationError(string? message = null, string? propertyName = null) 
    {
        PropertyName = propertyName;
        Message = message;

        Message ??= propertyName is null
            ? "Validation failed"
            : $"Validation failed for {propertyName}";
    }

    public override string ToString()
    {
        return new ReasonStringBuilder()
            .WithReasonType(GetType())
            .WithInfo(nameof(Message), Message)
            .WithInfo(nameof(PropertyName), PropertyName)
            .WithInfo(nameof(Metadata), string.Join("; ", Metadata))
            .WithInfo(nameof(Reasons), string.Join("; ", Reasons))
            .Build();
    }
}
