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

    public async Task<IReadOnlyList<Expense>> ListActiveExpensesAsync()
    {
        using var connection = _db.CreateConnection();

        var expenses = await connection.QueryAsync<Expense>("""
        SELECT *
        FROM Expenses
        WHERE DeletedAt IS NULL
        ORDER BY ExpenseDate DESC, CreatedAt DESC;
        """);

        return expenses.ToList();
    }

    public async Task<IReadOnlyList<ExpenseLineItem>> ListActiveLineItemsAsync()
    {
        using var connection = _db.CreateConnection();

        var lineItems = await connection.QueryAsync<ExpenseLineItem>("""
        SELECT *
        FROM ExpenseLineItems
        WHERE DeletedAt IS NULL;
        """);

        return lineItems.ToList();
    }

    public async Task<int> GetUnmatchedBankTransactionCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>("""
        SELECT COUNT(*)
        FROM BankTransactions bt
        WHERE bt.DeletedAt IS NULL
          AND NOT EXISTS (
              SELECT 1
              FROM ExpenseMatches em
              WHERE em.BankTransactionId = bt.BankTransactionId
                AND em.DeletedAt IS NULL
          );
        """);
    }

    public async Task<int> GetTotalBankTransactionCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>("""
        SELECT COUNT(*)
        FROM BankTransactions
        WHERE DeletedAt IS NULL;
        """);
    }

    public async Task<int> GetUnmatchedExpenseCountAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>("""
        SELECT COUNT(*)
        FROM Expenses e
        WHERE e.DeletedAt IS NULL
          AND NOT EXISTS (
              SELECT 1
              FROM ExpenseMatches em
              WHERE em.ExpenseId = e.ExpenseId
                AND em.DeletedAt IS NULL
          );
        """);
    }
}