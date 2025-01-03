namespace Sandbox.FullStackIdentity.Contracts;

public record ChangeEmailResponse(Guid UserId, string NewEmail, string? Status = null);