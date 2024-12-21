namespace Sandbox.FullStackIdentity.Domain;

public class MultitenantEntity : SoftEntity, IMultitenantEntity
{
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
