using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Contracts;

public record UserResponse(
    string Email, 
    bool InvitationAccepted,
    OrganizationPermission GrantedPermission,
    string? FirstName = null, 
    string? LastName = null);
