using FluentResults;
using Hope.Results;

namespace Sandbox.FullStackIdentity.Domain;

public interface ITenantRepository
{
    Task<Tenant?> GetAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Tenant?> FindByHandleAsync(string handle, CancellationToken cancellationToken = default);
    Task<PagedList<User>> ListUsersAsync(Guid? tenantId = null, PaginationParams? paginationParams = null, CancellationToken cancellationToken = default);

    Task<Result<Tenant>> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Result> ChangeNameAsync(string name, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Result> ChangeHandleAsync(string handle, Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task<Result<string[]>> AddToBlacklistAsync(string[] emails, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Result<string[]>> RemoveFromBlacklistAsync(string[] emails, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Result<string[]>> UpdateBlacklistAsync(string[] blacklistedEmails, Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<Result> DeletePermanentlyAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
}
