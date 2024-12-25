﻿using FluentResults;
using Hope.Results;

namespace Sandbox.FullStackIdentity.Domain;

public interface IBookRepository
{
    /// <summary>
    /// Retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="bookId">The unique identifier of the book.</param>
    /// <param name="loadOwner">Indicates whether to load the owner of the book.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The book if found, otherwise null.</returns>
    Task<Book?> GetAsync(Guid bookId, bool loadOwner = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a book by its title.
    /// </summary>
    /// <param name="title">The book title.</param>
    /// <param name="loadOwner">Indicates whether to load the owner of the book.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The book if found, otherwise null.</returns>
    Task<Book?> GetByTitleAsync(string title, bool loadOwner = false, CancellationToken cancellationToken = default);


    /// <summary>
    /// Lists all books.
    /// </summary>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <param name="deleted"><see langword="true"/> to list deleted books only, and <see langword="false"/> to list existing books only.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of books.</returns>
    Task<PagedList<Book>> ListAsync(PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists books by their owner's unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the owner.</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <param name="deleted"><see langword="true"/> to list deleted books only, and <see langword="false"/> to list existing books only.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of books created by the specified user.</returns>
    Task<PagedList<Book>> ListByOwnerAsync(Guid userId, PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default);


    /// <summary>
    /// Creates a new book.
    /// </summary>
    /// <param name="book">The book to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the created book.</returns>
    Task<Result<Book>> CreateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    /// <param name="bookId">The unique identifier of the book.</param>
    /// <param name="book">The updated book.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the updated book.</returns>
    Task<Result<Book>> UpdateAsync(Guid bookId, Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a book.
    /// </summary>
    /// <param name="bookId">The unique identifier of the book to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid bookId, CancellationToken cancellationToken = default);
}
