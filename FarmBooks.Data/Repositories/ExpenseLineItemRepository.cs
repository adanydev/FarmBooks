using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class ExpenseLineItemRepository
{
    private readonly DbConnectionFactory _db;

    public ExpenseLineItemRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(ExpenseLineItem item)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO ExpenseLineItems (
                ExpenseLineItemId, ExpenseId, CodeId, Description,
                Total, CreatedAt, UpdatedAt, DeletedAt
            )
            VALUES (
                @ExpenseLineItemId, @ExpenseId, @CodeId, @Description,
                @Total, @CreatedAt, @UpdatedAt, @DeletedAt
            );
            """,
            item
        );
    }

    public async Task<IReadOnlyList<ExpenseLineItem>> ListForExpenseAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                ExpenseLineItemId,
                ExpenseId,
                CodeId,
                Description,
                CAST(Total AS REAL) AS Total,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM ExpenseLineItems
            WHERE ExpenseId = @expenseId
              AND DeletedAt IS NULL
            ORDER BY CreatedAt;
            """;

        var items = await connection.QueryAsync<ExpenseLineItem>(sql, new { expenseId });

        return items.ToList();
    }

    public async Task SoftDeleteAsync(string expenseLineItemId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE ExpenseLineItems
            SET DeletedAt = @Now,
                UpdatedAt = @Now
            WHERE ExpenseLineItemId = @ExpenseLineItemId
              AND DeletedAt IS NULL;
            """,
            new { ExpenseLineItemId = expenseLineItemId, Now = DateTime.UtcNow }
        );
    }

    public async Task UpdateAsync(ExpenseLineItem item)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE ExpenseLineItems
            SET
                CodeId = @CodeId,
                Description = @Description,
                Total = @Total,
                UpdatedAt = @UpdatedAt
            WHERE ExpenseLineItemId = @ExpenseLineItemId
              AND DeletedAt IS NULL;
            """,
            item
        );
    }
}
