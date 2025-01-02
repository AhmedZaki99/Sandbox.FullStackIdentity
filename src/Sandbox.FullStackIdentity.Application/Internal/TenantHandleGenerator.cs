using System.Security.Cryptography;

namespace Sandbox.FullStackIdentity.Application;

internal sealed class TenantHandleGenerator : ITenantHandleGenerator
{
    private const int StandardLength = 8;
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyz123456789";

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var handle = RandomNumberGenerator.GetString(AllowedChars, StandardLength);
        return Task.FromResult(handle);
    }
}
