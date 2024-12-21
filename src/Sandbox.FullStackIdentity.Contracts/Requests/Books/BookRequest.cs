using System.ComponentModel.DataAnnotations;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class BookRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string Author { get; set; } = null!;
    
    public string? Publisher { get; set; }
    public DateTime? PublishDate { get; set; }
    public PublicationStatus? PublicationStatus { get; set; }
    public int? PagesCount { get; set; }
}
