namespace Sandbox.FullStackIdentity.Domain;

public interface IMultitenantEntity
{
    Guid? TenantId { get; set; }
}
