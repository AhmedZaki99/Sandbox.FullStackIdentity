using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class BookAppService : IBookAppService
{

    #region Dependencies

    private readonly IBookRepository _bookRepository;

    public BookAppService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<PagedList<BookResponse>> ListAsync(int page = 1, int pageSize = 30, Guid? ownerId = null, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);

        var pageResult = ownerId.HasValue
            ? await _bookRepository.ListByOwnerAsync(ownerId.Value, paginationParams, deleted: false, cancellationToken)
            : await _bookRepository.ListAsync(paginationParams, deleted: false, cancellationToken);

        return pageResult.MapItems(book => book.ToResponse());
    }

    /// <inheritdoc/>
    public async Task<PagedList<BookResponse>> ListDeletedAsync(int page = 1, int pageSize = 30, Guid? ownerId = null, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);

        var pageResult = ownerId.HasValue
            ? await _bookRepository.ListByOwnerAsync(ownerId.Value, paginationParams, deleted: true, cancellationToken)
            : await _bookRepository.ListAsync(paginationParams, deleted: true, cancellationToken);
        
        return pageResult.MapItems(book => book.ToResponse());
    }


    /// <inheritdoc/>
    public async Task<BookResponse?> GetAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetAsync(bookId, cancellationToken: cancellationToken);

        return book?.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<Result<BookResponse>> CreateAsync(Guid ownerId, BookRequest request, CancellationToken cancellationToken = default)
    {
        var book = request.ToBook();
        book.OwnerId = ownerId;

        var result = await _bookRepository.CreateAsync(book, cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        return result.Value.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<Result<BookResponse>> UpdateAsync(Guid bookId, BookRequest request, CancellationToken cancellationToken = default)
    {
        var book = request.ToBook();

        var result = await _bookRepository.UpdateAsync(bookId, book, cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        return result.Value.ToResponse();
    }

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return _bookRepository.DeleteAsync(bookId, cancellationToken);
    }

    #endregion

}
