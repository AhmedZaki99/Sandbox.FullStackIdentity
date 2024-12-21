namespace Sandbox.FullStackIdentity.Domain;

/// <summary>
/// An owned entity that contains extra details about a book.
/// </summary>
public class BookDetails
{
    public required string Author { get; set; }

    public string? Publisher { get; set; }
    public DateTime? PublishDate { get; set; }
    public PublicationStatus? PublicationStatus { get; set; }    
    public int? PagesCount { get; set; }
}
