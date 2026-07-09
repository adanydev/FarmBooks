using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class ExpenseMatchRepository
{
    private readonly DbConnectionFactory _db;

    public ExpenseMatchRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(ExpenseMatch match)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO ExpenseMatches (
            ExpenseMatchId,
            ExpenseId,
            BankTransactionId,
            MatchedAt,
            Notes,
            CreatedAt,
            DeletedAt
        )
        VALUES (
            @ExpenseMatchId,
            @ExpenseId,
            @BankTransactionId,
            @MatchedAt,
            @Notes,
            @CreatedAt,
            @DeletedAt
        );
        """, match);
    }

    public async Task SoftDeleteByBankTransactionAsync(string bankTransactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE ExpenseMatches
        SET DeletedAt = @Now
        WHERE BankTransactionId = @BankTransactionId
          AND DeletedAt IS NULL;
        """, new
        {
            BankTransactionId = bankTransactionId,
            Now = DateTime.UtcNow
        });
    }

    public async Task<IReadOnlyList<ExpenseMatch>> ListActiveAsync()
    {
        using var connection = _db.CreateConnection();

        var matches = await connection.QueryAsync<ExpenseMatch>("""
        SELECT *
        FROM ExpenseMatches
        WHERE DeletedAt IS NULL
        ORDER BY MatchedAt DESC;
        """);

        return matches.ToList();
    }

    public async Task<bool> IsBankTransactionMatchedAsync(string bankTransactionId)
    {
        using var connection = _db.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>("""
        SELECT COUNT(*)
        FROM ExpenseMatches
        WHERE BankTransactionId = @BankTransactionId
          AND DeletedAt IS NULL;
        """, new { BankTransactionId = bankTransactionId });

        return count > 0;
    }

    public async Task<bool> IsExpenseMatchedAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>("""
        SELECT COUNT(*)
        FROM ExpenseMatches
        WHERE ExpenseId = @ExpenseId
          AND DeletedAt IS NULL;
        """, new { ExpenseId = expenseId });

        return count > 0;
    }
}