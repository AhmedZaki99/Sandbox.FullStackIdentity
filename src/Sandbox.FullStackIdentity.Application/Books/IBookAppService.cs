using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Contracts;

namespace Sandbox.FullStackIdentity.Application;

public interface IBookAppService
{
    Task<PagedList<BookResponse>> ListAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<PagedList<BookResponse>> ListAsync(Guid creatorId, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<PagedList<BookResponse>> ListAsync(string senderEmail, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<PagedList<BookResponse>> ListDeletedAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default);

    Task<BookResponse?> GetAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<Result<BookResponse>> UpdateAsync(Guid bookId, BookRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task<Result<BookResponse>> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);
    Task<Result<BookResponse>> CreateAsync(BookRequest request, Guid creatorId, CancellationToken cancellationToken = default);
    Task<Result<BookResponse>> CreateAsync(BookRequest request, string senderEmail, CancellationToken cancellationToken = default);
}
