namespace Sandbox.FullStackIdentity.Domain;

public abstract class SoftEntity : EntityBase, ISoftEntity
{
    public bool IsDeleted { get; set; }
}
