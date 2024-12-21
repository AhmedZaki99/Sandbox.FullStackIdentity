using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Organization)]
[Route("api/organization")]
public sealed class OrganizationController : ControllerBase
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly IBearerTokenGenerator _bearerTokenGenerator;
    private readonly IOrganizationAppService _organizationAppService;
    private readonly IAccountEmailsAppService _accountEmailsAppService;
    private readonly IOptions<DataProtectionTokenProviderOptions> _dataProtectionTokenProviderOptions;

    public OrganizationController(
        AppUserManager userManager,
        IBearerTokenGenerator bearerTokenGenerator,
        IOrganizationAppService organizationAppService,
        IAccountEmailsAppService accountEmailsAppService,
        IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions)
    {
        _userManager = userManager;
        _bearerTokenGenerator = bearerTokenGenerator;
        _organizationAppService = organizationAppService;
        _accountEmailsAppService = accountEmailsAppService;
        _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions;
    }

    #endregion

    #region Actions

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return BadRequest("User is already logged in.");
        }

        var result = await _organizationAppService.CreateOrganizationAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }

        var user = result.Value;
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        await _accountEmailsAppService.SendEmailConfirmationCodeAsync(user, code, cancellationToken);

        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            var registerResponse = new RegisterResponse(user.Id, user.Email, Status: "Email confirmation code has been sent.");
            return Ok(registerResponse);
        }
        
        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return Ok(tokenResponse);
    }


    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.CreateInvitedUserAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        var user = result.Value;

        var code = await _userManager.GenerateInvitationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var codeExpirationDays = (int)Math.Floor(_dataProtectionTokenProviderOptions.Value.TokenLifespan.TotalDays);

        await _accountEmailsAppService.SendInvitationLinkAsync(
            user, 
            request.InvitationLinkPath, 
            code,
            codeExpirationDays,
            cancellationToken);

        return Ok();
    }

    [HttpPost("cancel-invitation")]
    public async Task<IActionResult> CancelInvitation([FromBody] InvitedUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.RemoveInvitedUserAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        return Ok();
    }

    [HttpPost("change-permissions")]
    public async Task<IActionResult> ChangePermissions([FromBody] InvitedUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.ChangeUserPermissionsAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        return Ok();
    }

    #endregion

}
