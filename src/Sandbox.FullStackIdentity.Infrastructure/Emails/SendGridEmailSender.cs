using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Application;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Sandbox.FullStackIdentity.Infrastructure;

public class SendGridEmailSender : IEmailSender
{

    #region Fields

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    #endregion

    #region Dependencies

    private readonly ISendGridClient _sendGridClient;
    private readonly IOptions<EmailSenderOptions> _emailSenderOptions;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        ISendGridClient sendGridClient,
        IOptions<EmailSenderOptions> emailSenderOptions,
        ILogger<SendGridEmailSender> logger)
    {
        _sendGridClient = sendGridClient;
        _emailSenderOptions = emailSenderOptions;
        _logger = logger;
    }

    #endregion

    #region Implementation

    public Task SendAsync(string email, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        var sendGridMessage = new SendGridMessage
        {
            From = new EmailAddress(_emailSenderOptions.Value.SenderEmailAddress, _emailSenderOptions.Value.SenderName),
            Subject = subject,
            HtmlContent = htmlMessage
        };
        sendGridMessage.AddTo(email);

        _logger.LogInformation("Sending email to {email} with subject {subject}:{message}", email, subject, Environment.NewLine + htmlMessage);

        return _sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
    }

    public Task SendAsync(string email, string templateKey, object? templateData = null, CancellationToken cancellationToken = default)
    {
        var templateId = _emailSenderOptions.Value.TemplatesIdMap.GetValueOrDefault(templateKey)
            ?? throw new ArgumentException($"Template Id for the key '{templateKey}' not found in '{EmailSenderOptions.Key}' configuration.");

        var sendGridMessage = new SendGridMessage
        {
            From = new EmailAddress(_emailSenderOptions.Value.SenderEmailAddress, _emailSenderOptions.Value.SenderName),
            TemplateId = templateId
        };
        sendGridMessage.AddTo(email);
        sendGridMessage.SetTemplateData(templateData);

        var serializedData = JsonSerializer.Serialize(templateData, _jsonSerializerOptions);
        _logger.LogInformation("Sending email to {email} with template {templateKey}:{templateData}", email, templateKey, Environment.NewLine + serializedData);

        return _sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
    }

    #endregion
}
