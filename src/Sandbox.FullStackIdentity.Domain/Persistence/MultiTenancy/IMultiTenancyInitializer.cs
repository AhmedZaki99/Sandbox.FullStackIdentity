namespace Sandbox.FullStackIdentity.Domain;

public interface IMultiTenancyInitializer
{
    void SetCurrentTenant(Guid? tenantId);
}
