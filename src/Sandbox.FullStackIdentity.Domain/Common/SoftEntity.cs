namespace Sandbox.FullStackIdentity.Domain;

public abstract class SoftEntity : EntityBase
{
    public bool IsDeleted { get; set; }
}
