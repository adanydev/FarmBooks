using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class DashboardRepository
{
    private readonly DbConnectionFactory _db;

    public DashboardRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Transaction>> ListActiveTransactionsAsync()
    {
        using var connection = _db.CreateConnection();

        var transactions = await connection.QueryAsync<Transaction>(
            """
            SELECT *
            FROM Transactions
            WHERE DeletedAt IS NULL
            ORDER BY TransactionDate DESC, CreatedAt DESC;
            """
        );

        return transactions.ToList();
    }

    public async Task<IReadOnlyList<TransactionLineItem>> ListActiveLineItemsAsync()
    {
        using var connection = _db.CreateConnection();

        var lineItems = await connection.QueryAsync<TransactionLineItem>(
            """
            SELECT *
            FROM TransactionLineItems
            WHERE DeletedAt IS NULL;
            """
        );

        return lineItems.ToList();
    }

    public async Task<int> GetUnmatchedBankTransactionCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM BankTransactions bt
            WHERE bt.DeletedAt IS NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM TransactionMatches em
                  WHERE em.BankTransactionId = bt.BankTransactionId
                    AND em.DeletedAt IS NULL
              );
            """
        );
    }

    public async Task<int> GetTotalBankTransactionCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM BankTransactions
            WHERE DeletedAt IS NULL;
            """
        );
    }

    public async Task<int> GetUnmatchedTransactionCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM Transactions e
            WHERE e.DeletedAt IS NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM TransactionMatches em
                  WHERE em.TransactionId = e.TransactionId
                    AND em.DeletedAt IS NULL
              );
            """
        );
    }
}
