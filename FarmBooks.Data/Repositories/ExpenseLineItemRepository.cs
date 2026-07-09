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

        await connection.ExecuteAsync("""
        INSERT INTO ExpenseLineItems (
            ExpenseLineItemId, ExpenseId, CodeId, Description,
            Total, VATTreatment, CreatedAt, UpdatedAt, DeletedAt
        )
        VALUES (
            @ExpenseLineItemId, @ExpenseId, @CodeId, @Description,
            @Total, @VATTreatment, @CreatedAt, @UpdatedAt, @DeletedAt
        );
        """, item);
    }

    public async Task<IReadOnlyList<ExpenseLineItem>> ListForExpenseAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        var items = await connection.QueryAsync<ExpenseLineItem>("""
        SELECT *
        FROM ExpenseLineItems
        WHERE ExpenseId = @ExpenseId
          AND DeletedAt IS NULL
        ORDER BY CreatedAt;
        """, new { ExpenseId = expenseId });

        return items.ToList();
    }

    public async Task SoftDeleteAsync(string expenseLineItemId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE ExpenseLineItems
        SET DeletedAt = @Now,
            UpdatedAt = @Now
        WHERE ExpenseLineItemId = @ExpenseLineItemId
          AND DeletedAt IS NULL;
        """, new
        {
            ExpenseLineItemId = expenseLineItemId,
            Now = DateTime.UtcNow
        });
    }
}