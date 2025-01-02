using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class ChangePasswordRequest
{
    [Required]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
