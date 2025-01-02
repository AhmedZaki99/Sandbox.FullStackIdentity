using Hope.Results;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public static class DomainToResponseMappingExtensions
{
    public static PagedList<TDestination> MapItems<TSource, TDestination>(this PagedList<TSource> pagedList, Func<TSource, TDestination> map)
    {
        return new PagedList<TDestination>
        {
            Items = pagedList.Items.Select(map).ToList(),
            TotalCount = pagedList.TotalCount,
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize,
        };
    } 

    public static BookResponse ToResponse(this Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.CreatorId,
            book.SenderEmail,
            book.Details?.ToResponse()
        );
    }

    public static BookDetailsResponse ToResponse(this BookDetails details)
    {
        return new BookDetailsResponse(
            details.Author,
            details.Publisher,
            details.PublishDate,
            details.PublicationStatus,
            details.PagesCount);
    }
}
