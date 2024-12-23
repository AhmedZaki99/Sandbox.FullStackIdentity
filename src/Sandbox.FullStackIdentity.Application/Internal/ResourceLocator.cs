using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Sandbox.FullStackIdentity.Application.Internal;

internal static class ResourceLocator
{
    public static Task<Stream> ReadResourceFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

        var fileStream = embeddedProvider.GetFileInfo(filePath).CreateReadStream();
        return Task.FromResult(fileStream);
    }


    public const string ResourcesDirectory = "Resources";

    public static class Templates
    {
        public static readonly string BasePath = Path.Combine(ResourcesDirectory, "Templates");

        public static class EN
        {
            public static readonly string EmailConfirmationCode = Path.Combine(BasePath, "en_email-confirmation-code.handlebars");
            public static readonly string AccountInvitationLink = Path.Combine(BasePath, "en_account-invitation-link.handlebars");
        }
    }
}
