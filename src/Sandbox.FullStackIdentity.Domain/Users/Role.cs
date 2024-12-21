using System.Diagnostics.CodeAnalysis;

namespace Sandbox.FullStackIdentity.Domain;

public class Role : EntityBase
{
    [NotNull]
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
}
