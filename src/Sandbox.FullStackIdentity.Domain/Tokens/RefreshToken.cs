namespace Sandbox.FullStackIdentity.Domain;

public class RefreshToken : EntityBase
{
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    public required string Token { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
}
