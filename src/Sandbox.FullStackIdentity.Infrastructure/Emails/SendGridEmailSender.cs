using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.Infrastructure;

public class SendGridEmailSender : IEmailSender
{
    public Task SendAsync(string email, string subject, string message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
