namespace Sandbox.FullStackIdentity.Domain;

public class LimitCrossedError : ValidationError
{
    public LimitCrossedError(string? message = null, string? propertyName = null) 
    {
        PropertyName = propertyName;
        Message = message;

        Message ??= propertyName is null
            ? "Allowed limit was crossed"
            : $"Allowed Limit was crossed for {propertyName}";
    }
}
