using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Domain;

public class User : IdentityUser<Guid>, IMultitenantEntity, ISoftEntity
{
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsInvited { get; set; }
    public bool InvitationAccepted { get; set; }
    public OrganizationPermission GrantedPermission { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }


    [NotNull]
    public override string? Email { get; set; }

    [NotNull]
    public override string? UserName { get; set; }
}
