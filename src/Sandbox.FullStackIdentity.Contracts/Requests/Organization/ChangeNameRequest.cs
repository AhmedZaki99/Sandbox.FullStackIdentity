using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class ChangeNameRequest
{
    [Required]
    public string Name { get; set; } = null!;
}
