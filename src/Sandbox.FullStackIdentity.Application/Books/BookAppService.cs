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
    public async Task<PagedList<BookResponse>> ListAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);
        var pageResult = await _bookRepository.ListAsync(paginationParams, deleted: false, cancellationToken);

        return pageResult.MapItems(book => book.ToResponse());
    }
    
    /// <inheritdoc/>
    public async Task<PagedList<BookResponse>> ListAsync(Guid creatorId, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);
        var pageResult = await _bookRepository.ListByCreatorAsync(creatorId, paginationParams, cancellationToken: cancellationToken);

        return pageResult.MapItems(book => book.ToResponse());
    }
    
    /// <inheritdoc/>
    public async Task<PagedList<BookResponse>> ListAsync(string senderEmail, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);
        var pageResult = await _bookRepository.ListBySenderEmailAsync(senderEmail, paginationParams, cancellationToken: cancellationToken);

        return pageResult.MapItems(book => book.ToResponse());
    }

    /// <inheritdoc/>
    public async Task<PagedList<BookResponse>> ListDeletedAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);
        var pageResult = await _bookRepository.ListAsync(paginationParams, deleted: true, cancellationToken);
        
        return pageResult.MapItems(book => book.ToResponse());
    }


    /// <inheritdoc/>
    public async Task<BookResponse?> GetAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetAsync(bookId, cancellationToken: cancellationToken);

        return book?.ToResponse();
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


    /// <inheritdoc/>
    public async Task<Result<BookResponse>> CreateAsync(BookRequest request, CancellationToken cancellationToken = default)
    {
        var book = request.ToBook();

        var result = await _bookRepository.CreateAsync(book, cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        return result.Value.ToResponse();
    }
    
    /// <inheritdoc/>
    public async Task<Result<BookResponse>> CreateAsync(BookRequest request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var book = request.ToBook();
        book.CreatorId = creatorId;

        var result = await _bookRepository.CreateAsync(book, cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        return result.Value.ToResponse();
    }
    
    /// <inheritdoc/>
    public async Task<Result<BookResponse>> CreateAsync(BookRequest request, string senderEmail, CancellationToken cancellationToken = default)
    {
        var book = request.ToBook();
        book.SenderEmail = senderEmail;

        var result = await _bookRepository.CreateAsync(book, cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        return result.Value.ToResponse();
    }

    #endregion

}
