using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class TransactionLineItemRepository
{
    private readonly DbConnectionFactory _db;

    public TransactionLineItemRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(TransactionLineItem item)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO TransactionLineItems (
                TransactionLineItemId, TransactionId, CodeId, Description,
                Total, StatementOrder, CreatedAt, UpdatedAt, DeletedAt
            )
            VALUES (
                @TransactionLineItemId, @TransactionId, @CodeId, @Description,
                @Total, @StatementOrder, @CreatedAt, @UpdatedAt, @DeletedAt
            );
            """,
            item
        );
    }

    public async Task<IReadOnlyList<TransactionLineItem>> ListForTransactionAsync(
        string transactionId
    )
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                TransactionLineItemId,
                TransactionId,
                CodeId,
                Description,
                CAST(Total AS REAL) AS Total,
                StatementOrder,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM TransactionLineItems
            WHERE TransactionId = @transactionId
              AND DeletedAt IS NULL
            ORDER BY StatementOrder, CreatedAt;
            """;

        var items = await connection.QueryAsync<TransactionLineItem>(sql, new { transactionId });

        return items.ToList();
    }

    public async Task SoftDeleteAsync(string transactionLineItemId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE TransactionLineItems
            SET DeletedAt = @Now,
                UpdatedAt = @Now
            WHERE TransactionLineItemId = @TransactionLineItemId
              AND DeletedAt IS NULL;
            """,
            new { TransactionLineItemId = transactionLineItemId, Now = DateTime.UtcNow }
        );
    }

    public async Task UpdateAsync(TransactionLineItem item)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE TransactionLineItems
            SET
                CodeId = @CodeId,
                Description = @Description,
                Total = @Total,
                StatementOrder = @StatementOrder,
                UpdatedAt = @UpdatedAt
            WHERE TransactionLineItemId = @TransactionLineItemId
              AND DeletedAt IS NULL;
            """,
            item
        );
    }
}
