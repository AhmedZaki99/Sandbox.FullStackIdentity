using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class ConfirmEmailRequest
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Code { get; set; } = null!;
}
