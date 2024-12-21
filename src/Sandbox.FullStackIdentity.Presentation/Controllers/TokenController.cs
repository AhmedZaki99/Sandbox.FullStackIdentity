using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandbox.FullStackIdentity.Contracts;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/token")]
public sealed class TokenController : ControllerBase
{

    #region Dependencies

    private readonly IBearerTokenGenerator _bearerTokenGenerator;

    public TokenController(IBearerTokenGenerator bearerTokenGenerator)
    {
        _bearerTokenGenerator = bearerTokenGenerator;
    }

    #endregion

    #region Actions

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<BearerTokenResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenResponse = await _bearerTokenGenerator.RefreshAsync(request.RefreshToken, cancellationToken);
        if (tokenResponse == null)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        return tokenResponse;
    }


    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        if (userId is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }

        await _bearerTokenGenerator.RevokeAsync(userId.Value, request.RefreshToken, cancellationToken);
        return NoContent();
    }
    
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAll(CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        if (userId is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }

        await _bearerTokenGenerator.RevokeAllAsync(userId.Value, cancellationToken);
        return NoContent();
    }

    #endregion

}
