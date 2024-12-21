using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Sandbox.FullStackIdentity.Application.Internal;

internal static class ResourceLocator
{

    private static readonly Assembly _currentAssembly = typeof(ResourceLocator).Assembly;

    public static Task<Stream> ReadResourceFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resourcePath = Path.Combine(ResourcesBasePath, fileName);
        var embeddedProvider = new EmbeddedFileProvider(_currentAssembly);

        var fileStream = embeddedProvider.GetFileInfo(resourcePath).CreateReadStream();
        return Task.FromResult(fileStream);
    }



    public const string ResourcesBasePath = "Resources";
    public const string TemplatesSubPath = "Templates";

    public static class EN
    {
        public const string EmailConfirmationCodeTemplate = TemplatesSubPath + "/email-confirmation-code.en.handlebars";
        public const string AccountInvitationLinkTemplate = TemplatesSubPath + "/account-invitation-link.en.handlebars";
    }

}
