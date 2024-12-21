using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
