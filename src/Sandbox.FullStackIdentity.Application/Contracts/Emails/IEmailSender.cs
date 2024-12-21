namespace Sandbox.FullStackIdentity.Application;

public interface IEmailSender
{
    Task SendAsync(string email, string subject, string message, CancellationToken cancellationToken = default);
}
