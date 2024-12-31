namespace Sandbox.FullStackIdentity.Domain;

public class Tenant : SoftEntity
{
    public required string Name { get; set; }
    public required string Handle { get; set; }

    public string[] BlacklistedEmails { get; set; } = [];
}
