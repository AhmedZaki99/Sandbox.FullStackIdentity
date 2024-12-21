namespace Sandbox.FullStackIdentity.Contracts;

public record RegisterResponse(Guid UserId, string Email, string Status);