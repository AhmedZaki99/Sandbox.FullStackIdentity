using Microsoft.Extensions.Logging;
using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.Infrastructure;

public class LoggerEmailSender : IEmailSender
{
    private readonly ILogger<LoggerEmailSender> _logger;

    public LoggerEmailSender(ILogger<LoggerEmailSender> logger)
    {
        _logger = logger;
    }


    public Task SendAsync(string email, string subject, string message, CancellationToken cancellationToken = default)
    {
        var emailMessage = 
            $"""

            ---------------Header---------------
            To: {email}
            Subject: {subject}
            -------------End-Header-------------
            ----------------Body----------------
            {message}
            --------------End-Body--------------

            """;

        _logger.LogInformation("Received email message: {emailMessage}", emailMessage);
        return Task.CompletedTask;
    }
}
