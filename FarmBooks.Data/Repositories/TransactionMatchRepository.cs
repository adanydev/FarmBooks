using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class TransactionMatchRepository
{
    private readonly DbConnectionFactory _db;

    public TransactionMatchRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(TransactionMatch match)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO TransactionMatches (
                TransactionMatchId,
                TransactionId,
                BankTransactionId,
                MatchedAt,
                Notes,
                CreatedAt,
                DeletedAt
            )
            VALUES (
                @TransactionMatchId,
                @TransactionId,
                @BankTransactionId,
                @MatchedAt,
                @Notes,
                @CreatedAt,
                @DeletedAt
            );
            """,
            match
        );
    }

    public async Task SoftDeleteByBankTransactionAsync(string bankTransactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE TransactionMatches
            SET DeletedAt = @Now
            WHERE BankTransactionId = @BankTransactionId
              AND DeletedAt IS NULL;
            """,
            new { BankTransactionId = bankTransactionId, Now = DateTime.UtcNow }
        );
    }

    public async Task<IReadOnlyList<TransactionMatch>> ListActiveAsync()
    {
        using var connection = _db.CreateConnection();

        var matches = await connection.QueryAsync<TransactionMatch>(
            """
            SELECT *
            FROM TransactionMatches
            WHERE DeletedAt IS NULL
            ORDER BY MatchedAt DESC;
            """
        );

        return matches.ToList();
    }

    public async Task<bool> IsBankTransactionMatchedAsync(string bankTransactionId)
    {
        using var connection = _db.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM TransactionMatches
            WHERE BankTransactionId = @BankTransactionId
              AND DeletedAt IS NULL;
            """,
            new { BankTransactionId = bankTransactionId }
        );

        return count > 0;
    }

    public async Task<bool> IsTransactionMatchedAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM TransactionMatches
            WHERE TransactionId = @TransactionId
              AND DeletedAt IS NULL;
            """,
            new { TransactionId = transactionId }
        );

        return count > 0;
    }
}
