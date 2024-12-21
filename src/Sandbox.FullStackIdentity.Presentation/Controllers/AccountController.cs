using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController : ControllerBase
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly IBearerTokenGenerator _bearerTokenGenerator;

    public AccountController(
        AppUserManager userManager,
        IBearerTokenGenerator bearerTokenGenerator)
    {
        _userManager = userManager;
        _bearerTokenGenerator = bearerTokenGenerator;
    }

    #endregion

    #region Actions

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<BearerTokenResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            ModelState.AddModelError("Login", "Invalid email or password.");
            return ValidationProblem(ModelState);
        }
        cancellationToken.ThrowIfCancellationRequested();

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError("Login", "Email is not confirmed.");
            return ValidationProblem(ModelState);
        }

        bool passwordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordCorrect)
        {
            ModelState.AddModelError("Login", "Invalid email or password.");
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }


    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<ActionResult<BearerTokenResponse>> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.ConfirmEmailAsync(user, request.Code);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }
    
    [AllowAnonymous]
    [HttpPost("accept-invitation")]
    public async Task<ActionResult<BearerTokenResponse>> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

        var confirmResult = await _userManager.ConfirmInvitedEmailAsync(user, token);
        if (!confirmResult.Succeeded)
        {
            foreach (var error in confirmResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var passwordResult = await _userManager.AddPasswordAsync(user, request.Password);
        if (!passwordResult.Succeeded)
        {
            foreach (var error in passwordResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }

    #endregion

}
