namespace Sandbox.FullStackIdentity.Domain;

public class Book : MultitenantEntity
{
    public Guid? CreatorId { get; set; }
    public User? Creator { get; set; }

    public string? SenderEmail { get; set; }
    public string? SentMessageId { get; set; }

    public required string Title { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public BookDetails? Details { get; set; }
}
