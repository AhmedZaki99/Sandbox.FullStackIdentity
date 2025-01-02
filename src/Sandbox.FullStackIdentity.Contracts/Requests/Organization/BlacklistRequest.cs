using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class BlacklistRequest
{
    [Required]
    public string[] Emails { get; set; } = null!;
}
