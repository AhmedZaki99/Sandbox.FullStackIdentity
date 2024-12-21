using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class AcceptInvitationRequest
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Token { get; set; } = null!;


    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
