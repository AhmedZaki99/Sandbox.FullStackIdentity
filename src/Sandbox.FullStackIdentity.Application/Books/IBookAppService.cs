using FluentResults;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface IBookAppService
{
    Task<PagedList<BookResponse>> ListAsync(int page = 1, int pageSize = 30, Guid? ownerId = null, CancellationToken cancellationToken = default);
    Task<PagedList<BookResponse>> ListDeletedAsync(int page = 1, int pageSize = 30, Guid? ownerId = null, CancellationToken cancellationToken = default);

    Task<BookResponse?> GetAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<Result<BookResponse>> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);
    Task<Result<BookResponse>> UpdateAsync(Guid bookId, BookRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid bookId, CancellationToken cancellationToken = default);
}
