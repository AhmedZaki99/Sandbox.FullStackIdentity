using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public class RoleRequest
{
    [Required]
    [MinLength(5)]
    public required string RoleName { get; set; }

    public Guid? IncludedUserId { get; set; }
}
