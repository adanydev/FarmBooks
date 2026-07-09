using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class BankRepository
{
    private readonly DbConnectionFactory _db;

    public BankRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAccountAsync(BankAccount account)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO BankAccounts (
            BankAccountId, Name, BankName, OpeningBalance,
            OpeningBalanceDate, IsActive, CreatedAt, UpdatedAt, DeletedAt
        )
        VALUES (
            @BankAccountId, @Name, @BankName, @OpeningBalance,
            @OpeningBalanceDate, @IsActive, @CreatedAt, @UpdatedAt, @DeletedAt
        );
        """, account);
    }

    public async Task<IReadOnlyList<BankAccount>> ListAccountsAsync()
    {
        using var connection = _db.CreateConnection();

        var accounts = await connection.QueryAsync<BankAccount>("""
        SELECT *
        FROM BankAccounts
        WHERE DeletedAt IS NULL
        ORDER BY Name;
        """);

        return accounts.ToList();
    }

    public async Task CreateStatementAsync(BankStatement statement)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO BankStatements (
            BankStatementId, BankAccountId, StatementStartDate, StatementEndDate,
            OpeningBalance, ClosingBalance, StatementNumber, Notes,
            CreatedAt, UpdatedAt, DeletedAt
        )
        VALUES (
            @BankStatementId, @BankAccountId, @StatementStartDate, @StatementEndDate,
            @OpeningBalance, @ClosingBalance, @StatementNumber, @Notes,
            @CreatedAt, @UpdatedAt, @DeletedAt
        );
        """, statement);
    }

    public async Task CreateTransactionAsync(BankTransaction transaction)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO BankTransactions (
            BankTransactionId, BankAccountId, BankStatementId, TransactionDate,
            Description, MoneyIn, MoneyOut, BalanceAfterTransaction,
            Reference, ExpenseId, CreatedAt, UpdatedAt, DeletedAt
        )
        VALUES (
            @BankTransactionId, @BankAccountId, @BankStatementId, @TransactionDate,
            @Description, @MoneyIn, @MoneyOut, @BalanceAfterTransaction,
            @Reference, @ExpenseId, @CreatedAt, @UpdatedAt, @DeletedAt
        );
        """, transaction);
    }

    public async Task<IReadOnlyList<BankTransaction>> ListTransactionsAsync()
    {
        using var connection = _db.CreateConnection();

        var transactions = await connection.QueryAsync<BankTransaction>("""
        SELECT *
        FROM BankTransactions
        WHERE DeletedAt IS NULL
        ORDER BY TransactionDate DESC, CreatedAt DESC;
        """);

        return transactions.ToList();
    }

    public async Task MatchExpenseAsync(string bankTransactionId, string ExpenseId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE BankTransactions
        SET ExpenseId = @ExpenseId,
            UpdatedAt = @Now
        WHERE BankTransactionId = @BankTransactionId
          AND DeletedAt IS NULL;
        """, new
        {
            BankTransactionId = bankTransactionId,
            ExpenseId = ExpenseId,
            Now = DateTime.UtcNow
        });
    }

    public async Task UnmatchExpenseAsync(string bankTransactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE BankTransactions
        SET ExpenseId = NULL,
            UpdatedAt = @Now
        WHERE BankTransactionId = @BankTransactionId
          AND DeletedAt IS NULL;
        """, new
        {
            BankTransactionId = bankTransactionId,
            Now = DateTime.UtcNow
        });
    }
}