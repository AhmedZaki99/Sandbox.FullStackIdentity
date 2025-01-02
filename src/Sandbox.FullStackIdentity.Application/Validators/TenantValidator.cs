using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class TenantValidator : ITenantValidator
{

    #region Dependencies

    private readonly ITenantRepository _tenantRepository;

    public TenantValidator(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Result> ValidateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var existingTenant = await _tenantRepository.FindByHandleAsync(tenant.Handle, cancellationToken);
        if (existingTenant is not null && tenant.Id != existingTenant.Id)
        {
            return new ConflictError("An organization with this handle already exists.");
        }
        return Result.Ok();
    }

    #endregion

}
