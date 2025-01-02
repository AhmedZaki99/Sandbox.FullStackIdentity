using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class ChangeEmailRequest
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = null!;
}
