using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public static class RequestToDomainMappingExtensions
{
    public static Book ToBook(this BookRequest request, Guid? bookId = null)
    {
        return new Book()
        {
            Id = bookId ?? Guid.Empty,
            Title = request.Title,
            Details = new BookDetails()
            {
                Author = request.Author,
                Publisher = request.Publisher,
                PublishDate = request.PublishDate,
                PublicationStatus = request.PublicationStatus,
                PagesCount = request.PagesCount
            }
        };
    }
}
