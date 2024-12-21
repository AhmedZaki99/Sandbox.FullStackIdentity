using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class MultiTenancyService : IMultiTenancyContext, IMultiTenancyInitializer
{
    public Guid? CurrentTenantId { get; private set; }

    public void SetCurrentTenant(Guid? tenantId)
    {
        CurrentTenantId = tenantId;
    }
}
