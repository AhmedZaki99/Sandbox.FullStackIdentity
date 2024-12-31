namespace Sandbox.FullStackIdentity.Domain;

public class SubscriptionPreference
{
    public required string Email { get; set; }

    public bool IsSubscribed { get; set; }
    public SubscriptionScope? Scope { get; set; }
}
