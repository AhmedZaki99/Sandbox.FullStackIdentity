namespace Sandbox.FullStackIdentity.Domain;

public interface IMultiTenancyContext
{
    Guid? CurrentTenantId { get; }
}
