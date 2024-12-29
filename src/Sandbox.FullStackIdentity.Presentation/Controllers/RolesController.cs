using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("api/roles")]
public class RolesController : ControllerBase
{

    #region Dependencies

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RolesController> _logger;

    #endregion

    #region Constructor

    public RolesController(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger<RolesController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    #endregion


    #region Actions

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        bool roleExist = await _roleManager.RoleExistsAsync(request.RoleName);
        if (roleExist)
        {
            return Conflict();
        }
        cancellationToken.ThrowIfCancellationRequested();

        var role = new IdentityRole<Guid>(request.RoleName);

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(result.ToString());
        }
        _logger.LogInformation("An admin has created a '{role}' role", request.RoleName);

        if (request.IncludedUserId?.ToString() is string includedUserId)
        {
            result = await AddUserToRoleAsync(includedUserId, request.RoleName, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result.ToString());
            }
            _logger.LogInformation("An admin has added the user '{userId}' to the '{role}' role", includedUserId, request.RoleName);
        }
        return Created();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        if (request.RoleName == ApplicationRoles.Administrator)
        {
            return BadRequest("Admins are not allowed to delete the admin role");
        }

        var roleToDelete = await _roleManager.FindByNameAsync(request.RoleName);
        if (roleToDelete is null)
        {
            return NotFound();
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _roleManager.DeleteAsync(roleToDelete);
        if (result.Succeeded)
        {
            _logger.LogInformation("An admin has deleted the '{role}' role", request.RoleName);
            return Ok();
        }
        return BadRequest(result.ToString());
    }


    [HttpPost("{roleName}/include/{userId}")]
    public async Task<IActionResult> Include(string roleName, string userId, CancellationToken cancellationToken)
    {
        bool roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            return NotFound();
        }

        var result = await AddUserToRoleAsync(userId, roleName, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result.ToString());
        }

        _logger.LogInformation("An admin has added the user '{userId}' to the '{role}' role", userId, roleName);
        return Ok();
    }

    [HttpPost("{roleName}/exclude/{userId}")]
    public async Task<IActionResult> Exclude(string roleName, string userId, CancellationToken cancellationToken)
    {
        bool roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            return NotFound();
        }

        var result = await RemoveUserFromRoleAsync(userId, roleName, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result.ToString());
        }

        _logger.LogInformation("An admin has removed the user '{userId}' from the '{role}' role", userId, roleName);
        return Ok();
    }

    #endregion


    #region Helpers

    private async Task<IdentityResult> AddUserToRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "User not found" });
        }
        cancellationToken.ThrowIfCancellationRequested();

        return await _userManager.AddToRoleAsync(user, role);
    }

    private async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "User not found" });
        }
        cancellationToken.ThrowIfCancellationRequested();

        return await _userManager.RemoveFromRoleAsync(user, role);
    }

    #endregion

}