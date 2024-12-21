using System.Diagnostics.CodeAnalysis;

namespace Sandbox.FullStackIdentity.Domain;

public class User : MultitenantEntity
{
    public bool IsInvited { get; set; }
    public bool InvitationAccepted { get; set; }
    public OrganizationPermission GrantedPermission { get; set; }

    [NotNull]
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }

    [NotNull]
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }
    public string? SecurityStamp { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
