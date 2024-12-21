using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("api/administration")]
public sealed class AdministrationController : ControllerBase
{

    #region Dependencies

    private readonly ITokenCleanupAppService _tokenCleanupAppService;
    private readonly ILogger<AdministrationController> _logger;

    public AdministrationController(
        ITokenCleanupAppService tokenCleanupAppService,
        ILogger<AdministrationController> logger)
    {
        _tokenCleanupAppService = tokenCleanupAppService;
        _logger = logger;
    }

    #endregion

    #region Actions

    [HttpPost("schedule-cleanup-job")]
    public async Task<IActionResult> ScheduleCleanupJob([FromBody] SchedulingRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        if (userId is null)
        {
            _logger.LogWarning("Unable to resolve the Id of an authorized user. UserName: {userName}", User.Identity?.Name);
            return Unauthorized();
        }

        bool successful = await _tokenCleanupAppService.ScheduleCleanupProcessAsync(request?.Cron, cancellationToken);
        if (!successful)
        {
            return request?.Cron is null
                ? BadRequest("The token cleanup job CRON expression has not been initialized yet, try to resend the request with a CRON expression in the body.")
                : BadRequest("The server failed to schedule the token cleanup job with the CRON expression provided.");
        }
        return Ok();
    }

    [HttpPost("cancel-cleanup-job")]
    public async Task<IActionResult> CancelCleanupJob(CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        if (userId is null)
        {
            _logger.LogWarning("Unable to resolve the Id of an authorized user. UserName: {userName}", User.Identity?.Name);
            return Unauthorized();
        }
        await _tokenCleanupAppService.CancelCleanupProcessAsync(cancellationToken);

        return Ok();
    }

    #endregion

}
