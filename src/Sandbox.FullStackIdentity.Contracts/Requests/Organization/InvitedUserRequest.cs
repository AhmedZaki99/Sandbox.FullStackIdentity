using System.ComponentModel.DataAnnotations;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class InvitedUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public OrganizationPermission GivenPermission { get; set; }
}
