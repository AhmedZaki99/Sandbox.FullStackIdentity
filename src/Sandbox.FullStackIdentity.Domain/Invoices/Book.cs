namespace Sandbox.FullStackIdentity.Domain;

public class Book : MultitenantEntity
{
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public required string Title { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public BookDetails? Details { get; set; }
}
