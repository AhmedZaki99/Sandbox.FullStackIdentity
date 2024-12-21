using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public interface ITenantRepository
{
    Task<Tenant?> GetAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Tenant?> FindByHandleAsync(string handle, CancellationToken cancellationToken = default);
    Task<List<User>> ListUsersAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task<Result<Tenant>> CreateAsync(string handle, string? name = null, CancellationToken cancellationToken = default);
    Task<Result> ChangeNameAsync(string name, Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Result> DeletePermanentlyAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
}
