using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <summary>
/// Provides functionalities for managing organizations.
/// </summary>
public interface IOrganizationAppService
{
    Task<PagedList<UserResponse>> ListUsersAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default);

    Task<Result<User>> CreateOrganizationAsync(RegisterRequest requestModel, CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> GenerateNewHandleAsync(CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> ChangeNameAsync(ChangeNameRequest request, CancellationToken cancellationToken = default);

    Task<Result<User>> CreateInvitedUserAsync(InviteRequest requestModel, CancellationToken cancellationToken = default);
    Task<Result> RemoveInvitedUserAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default);    
    Task<Result> ChangeUserPermissionsAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default);

    Task<Result<OrganizationResponse>> AddEmailToBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> RemoveEmailFromBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> UpdateBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default);
}
