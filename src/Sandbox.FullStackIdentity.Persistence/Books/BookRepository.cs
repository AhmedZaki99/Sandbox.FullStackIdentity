using System.Data.Common;
using System.Text;
using System.Text.Json;
using Dapper;
using FluentResults;
using Hope.Identity.Dapper;
using Hope.Results;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class BookRepository : IBookRepository
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;
    private readonly IMultiTenancyContext _multiTenancyContext;

    public BookRepository(DbDataSource dbDataSource, IMultiTenancyContext multiTenancyContext)
    {
        _dbDataSource = dbDataSource;
        _multiTenancyContext = multiTenancyContext;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Book?> GetAsync(Guid bookId, bool loadCreator = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder("SELECT * FROM books b");
        if (loadCreator)
        {
            sqlBuilder.AppendLine("LEFT JOIN identity.users u ON b.creator_id = u.id");
        }
        sqlBuilder.AppendLine("WHERE b.tenant_id = @tenantId AND b.id = @bookId LIMIT 1");

        var param = new { tenantId, bookId };
        var result = loadCreator
            ? await connection.QueryAsync(sqlBuilder.ToString(), GetBookMap(), param, splitOn: "id")
            : await connection.QueryAsync<Book>(sqlBuilder.ToString(), param);

        return result.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Book?> GetByTitleAsync(string title, bool loadCreator = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder("SELECT * FROM books b");
        if (loadCreator)
        {
            sqlBuilder.AppendLine("LEFT JOIN identity.users u ON b.creator_id = u.id");
        }
        sqlBuilder.AppendLine("WHERE b.tenant_id = @tenantId AND b.title = @title LIMIT 1");

        var param = new { tenantId, title };
        var result = loadCreator
            ? await connection.QueryAsync(sqlBuilder.ToString(), GetBookMap(), param, splitOn: "id")
            : await connection.QueryAsync<Book>(sqlBuilder.ToString(), param);

        return result.FirstOrDefault();
    }


    /// <inheritdoc/>
    public Task<PagedList<Book>> ListAsync(PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;

        var sqlCondition = GetBaseCondition("tenantId", deleted: deleted);
        var parameters = new DynamicParameters(new { tenantId });

        return PaginateAsync(sqlCondition, parameters, paginationParams, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PagedList<Book>> ListByCreatorAsync(Guid userId, PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;

        var sqlCondition = $"{GetBaseCondition("tenantId", deleted: deleted)} AND creator_id = @userId";
        var parameters = new DynamicParameters(new { tenantId, userId });

        return PaginateAsync(sqlCondition, parameters, paginationParams, cancellationToken);
    }
    
    /// <inheritdoc/>
    public Task<PagedList<Book>> ListBySenderEmailAsync(string email, PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;

        var sqlCondition = $"{GetBaseCondition("tenantId", deleted: deleted)} AND sender_email = @email";
        var parameters = new DynamicParameters(new { tenantId, email });

        return PaginateAsync(sqlCondition, parameters, paginationParams, cancellationToken);
    }
    

    /// <inheritdoc/>
    public async Task<Result<Book>> CreateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        if (book.CreatorId is not null)
        {
            bool userExists = await connection.ExecuteScalarAsync<bool>(
                $"""
                SELECT EXISTS (
                    SELECT 1 FROM identity.users WHERE tenant_id = @{nameof(Book.TenantId)} AND id = @{nameof(Book.CreatorId)}
                )
                """,
                book);

            if (!userExists)
            {
                return new NotFoundError("Owner not found");
            }
        }

        if (book.Id == Guid.Empty)
        {
            book.Id = Guid.NewGuid();
        }
        book.TenantId = tenantId;

        string[] propertyNames = [
            nameof(Book.Id),
            nameof(Book.TenantId),
            nameof(Book.CreatorId),
            nameof(Book.SenderEmail),
            nameof(Book.SentMessageId),
            nameof(Book.Title),
            nameof(Book.Details)
        ];

        await connection.ExecuteAsync(
            $"""
            INSERT INTO books {propertyNames.BuildSqlColumnsBlock(JsonNamingPolicy.SnakeCaseLower, insertLines: true)} 
            VALUES {propertyNames.BuildSqlParametersBlock(insertLines: true)}
            """,
            book);

        return book;
    }

    /// <inheritdoc/>
    public async Task<Result<Book>> UpdateAsync(Guid bookId, Book book, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        book.Id = bookId;
        book.TenantId = tenantId;

        string[] propertyNames = [
            nameof(Book.Title),
            nameof(Book.Details),
            nameof(Book.IsDeleted)
        ];

        int count = await connection.ExecuteAsync(
            $"""
            UPDATE books 
            SET {propertyNames.BuildSqlColumnsBlock(JsonNamingPolicy.SnakeCaseLower)}  = {propertyNames.BuildSqlParametersBlock()}
            WHERE {GetBaseCondition(nameof(Book.TenantId))} AND id = @{nameof(Book.Id)}
            """,
            book);

        if (count < 1)
        {
            return new NotFoundError("Book is not found.");
        }
        return book;
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            $"""
            UPDATE books SET is_deleted = TRUE 
            WHERE {GetBaseCondition("tenantId")} AND id = @bookId
            """,
            new { tenantId, bookId });

        if (count < 1)
        {
            return new NotFoundError("Book is not found.");
        }
        return Result.Ok();
    }

    #endregion

    #region Helpres

    public async Task<PagedList<Book>> PaginateAsync(string queryCondition, DynamicParameters parameters, PaginationParams? paginationParams, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        paginationParams ??= new();
        parameters.Add("pageSize", paginationParams.PageSize);
        parameters.Add("offset", paginationParams.Offset);

        var sql =
            $"""
            SELECT COUNT(*) FROM books
            WHERE {queryCondition};

            SELECT * FROM books 
            WHERE {queryCondition}
            ORDER BY created_on_utc DESC
            LIMIT @pageSize OFFSET @offset;
            """;

        using var multi = await connection.QueryMultipleAsync(sql, parameters);

        var count = await multi.ReadFirstAsync<int>();
        var items = await multi.ReadAsync<Book>();

        return new PagedList<Book>
        {
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = count,
            Items = items.ToList()
        };
    }


    private static Func<Book, User, Book> GetBookMap()
    {
        return (book, user) =>
        {
            book.Creator = user;
            return book;
        };
    }

    private static string GetBaseCondition(string tenantIdParam, string? tableName = null, bool deleted = false)
    {
        var tablePrefix = tableName is not null 
            ? tableName + "." 
            : string.Empty;

        return $"{tablePrefix}is_deleted = {deleted} AND {tablePrefix}tenant_id = @{tenantIdParam}";
    }

    #endregion

}
