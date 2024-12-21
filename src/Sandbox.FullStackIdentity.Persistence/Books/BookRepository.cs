using System.Data.Common;
using System.Text;
using Dapper;
using FluentResults;
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
    public async Task<Book?> GetAsync(Guid bookId, bool loadOwner = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder("SELECT * FROM books b");
        if (loadOwner)
        {
            sqlBuilder.AppendLine("INNER JOIN users u ON b.owner_id = u.id");
        }
        sqlBuilder.AppendLine("WHERE b.tenant_id = @tenantId AND b.id = @bookId LIMIT 1");

        var param = new { tenantId, bookId };
        var result = loadOwner
            ? await connection.QueryAsync(sqlBuilder.ToString(), GetBookMap(), param, splitOn: "id")
            : await connection.QueryAsync<Book>(sqlBuilder.ToString(), param);

        return result.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Book?> GetByTitleAsync(string title, bool loadOwner = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder("SELECT * FROM books b");
        if (loadOwner)
        {
            sqlBuilder.AppendLine("INNER JOIN users u ON b.owner_id = u.id");
        }
        sqlBuilder.AppendLine("WHERE b.tenant_id = @tenantId AND b.title = @title LIMIT 1");

        var param = new { tenantId, title };
        var result = loadOwner
            ? await connection.QueryAsync(sqlBuilder.ToString(), GetBookMap(), param, splitOn: "id")
            : await connection.QueryAsync<Book>(sqlBuilder.ToString(), param);

        return result.FirstOrDefault();
    }


    /// <inheritdoc/>
    public async Task<PagedList<Book>> ListAsync(PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        paginationParams ??= new();

        var condition = GetBaseCondition("tenantId", deleted: deleted);
        var param = new
        {
            tenantId,
            pageSize = paginationParams.PageSize,
            offset = paginationParams.Offset
        };

        var (count, items) = await PaginateAsync(connection, condition, param);

        return new PagedList<Book>
        {
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = count,
            Items = items.ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<PagedList<Book>> ListByOwnerAsync(Guid userId, PaginationParams? paginationParams = null, bool deleted = false, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        paginationParams ??= new();

        var condition = $"{GetBaseCondition("tenantId", deleted: deleted)} AND owner_id = @userId";
        var param = new
        {
            tenantId,
            userId,
            pageSize = paginationParams.PageSize,
            offset = paginationParams.Offset
        };

        var (count, items) = await PaginateAsync(connection, condition, param);

        return new PagedList<Book>
        {
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = count,
            Items = items.ToList()
        };
    }
    

    /// <inheritdoc/>
    public async Task<Result<Book>> CreateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var tenantId = _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);
        
        if (book.OwnerId == Guid.Empty)
        {
            return new ValidationError("Owner Id is required", nameof(book.OwnerId));
        }
        if (book.Id == Guid.Empty)
        {
            book.Id = Guid.NewGuid();
        }
        book.TenantId = tenantId;
        
        bool userExists = connection.ExecuteScalar<bool>(
            $"""
            SELECT COUNT(1) FROM users WHERE tenant_id = @{nameof(Book.TenantId)} AND id = @{nameof(Book.OwnerId)}
            """,
            book);

        if (!userExists)
        {
            return new NotFoundError("Owner not found");
        }
        await connection.ExecuteAsync(
            $"""
            INSERT INTO books (id, tenant_id, owner_id, title, details) 
            VALUES (
                @{nameof(Book.Id)},
                @{nameof(Book.TenantId)},
                @{nameof(Book.OwnerId)},
                @{nameof(Book.Title)},
                @{nameof(Book.Details)}
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

        int count = await connection.ExecuteAsync(
            $"""
            UPDATE books 
            SET (title, details, is_deleted) = (
                @{nameof(Book.Title)}, 
                @{nameof(Book.Details)}),
                @{nameof(Book.IsDeleted)})
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

    private static async Task<(int, IEnumerable<Book>)> PaginateAsync(DbConnection connection, string queryCondition, object param)
    {
        var sql =
            $"""
            SELECT COUNT(*) FROM books
            WHERE {queryCondition};

            SELECT * FROM books 
            WHERE {queryCondition}
            ORDER BY created_on_utc DESC
            LIMIT @pageSize OFFSET @offset;
            """;

        using var multi = await connection.QueryMultipleAsync(sql, param);

        var count = await multi.ReadFirstAsync<int>();
        var items = await multi.ReadAsync<Book>();

        return (count, items);
    }


    private static Func<Book, User, Book> GetBookMap()
    {
        return (book, user) =>
        {
            book.Owner = user;
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
