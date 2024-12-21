namespace Sandbox.FullStackIdentity.Domain;

public class Tenant : SoftEntity
{
    public string? Name { get; set; }
    public required string Handle { get; set; }
}
