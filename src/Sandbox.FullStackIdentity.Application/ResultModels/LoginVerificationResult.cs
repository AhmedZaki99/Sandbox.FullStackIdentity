using System.Diagnostics.CodeAnalysis;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public sealed class LoginVerificationResult
{
    public static LoginVerificationResult Success(User user) => new(succeeded: true, user: user);
    public static LoginVerificationResult Fail(string errorMessage) => new(succeeded: false, errorMessage);


    [MemberNotNullWhen(true, nameof(User))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Succeeded { get; set; }

    public string? ErrorMessage { get; set; }
    public User? User { get; set; }

    private LoginVerificationResult(bool succeeded, string? errorMessage = null, User? user = null)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        User = user;
    }
}
