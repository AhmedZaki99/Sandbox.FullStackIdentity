namespace Sandbox.FullStackIdentity.Contracts;

public record UserDetailsResponse(
    string Email, 
    bool EmailConfirmed, 
    bool TwoFactorEnabled, 
    string? FirstName = null, 
    string? LastName = null,
    string? OrganizationName = null, 
    string? OrganizationHandle = null);
