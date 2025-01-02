using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class UserEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
