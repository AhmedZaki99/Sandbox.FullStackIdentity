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

    
    public static OrganizationResponse ToResponse(this Tenant tenant)
    {
        return new OrganizationResponse(
            tenant.Name,
            tenant.Handle,
            tenant.BlacklistedEmails
        );
    }
    
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Email,
            user.InvitationAccepted,
            user.GrantedPermission,
            user.FirstName,
            user.LastName
        );
    }
    
    public static UserDetailsResponse ToDetailsResponse(this User user, Tenant? tenant)
    {
        return new UserDetailsResponse(
            user.Email,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            user.FirstName,
            user.LastName,
            OrganizationName: tenant?.Name,
            OrganizationHandle: tenant?.Handle
        );
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
