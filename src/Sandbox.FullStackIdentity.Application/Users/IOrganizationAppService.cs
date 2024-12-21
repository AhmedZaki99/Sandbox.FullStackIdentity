using FluentResults;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <summary>
/// Provides functionalities for managing organizations.
/// </summary>
public interface IOrganizationAppService
{
    Task<Result<User>> CreateOrganizationAsync(RegisterRequest requestModel, CancellationToken cancellationToken = default);

    Task<Result<User>> CreateInvitedUserAsync(InviteRequest requestModel, CancellationToken cancellationToken = default);
    Task<Result> RemoveInvitedUserAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default);    
    Task<Result> ChangeUserPermissionsAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default);
}
