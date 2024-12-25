using Hope.Configuration;
using SendGrid.Helpers.Reliability;

namespace Sandbox.FullStackIdentity.Infrastructure;

public sealed class EmailSenderOptions : IKeyedOptions
{
    public const string Key = "EmailSender";
    string IKeyedOptions.Key => Key;


    public string? SenderName { get; set; }
    public string SenderEmailAddress { get; set; } = string.Empty;

    public Dictionary<string, string> TemplatesIdMap { get; set; } = [];

    public ReliabilitySettings ReliabilitySettings { get; set; } = new();
}
