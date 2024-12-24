namespace Sandbox.FullStackIdentity.Application;

public interface IEmailSender
{
    Task SendAsync(string email, string subject, string htmlMessage, CancellationToken cancellationToken = default);
    Task SendAsync(string email, string templateKey, object? templateData = null, CancellationToken cancellationToken = default);
}
