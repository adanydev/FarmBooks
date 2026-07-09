using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class BankService
{
    private readonly BankRepository _bankRepository;

    public BankService(BankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<string> CreateAccountAsync(
        string name,
        string? bankName,
        decimal openingBalance,
        DateTime openingBalanceDate)
    {
        var now = DateTime.UtcNow;

        var account = new BankAccount
        {
            BankAccountId = Guid.NewGuid().ToString(),
            Name = name.Trim(),
            BankName = bankName,
            OpeningBalance = openingBalance,
            OpeningBalanceDate = openingBalanceDate,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _bankRepository.CreateAccountAsync(account);

        return account.BankAccountId;
    }

    public Task<IReadOnlyList<BankAccount>> ListAccountsAsync()
    {
        return _bankRepository.ListAccountsAsync();
    }

    public async Task<string> CreateStatementAsync(
        string bankAccountId,
        DateTime startDate,
        DateTime endDate,
        decimal? openingBalance,
        decimal closingBalance,
        string? statementNumber,
        string? notes)
    {
        var now = DateTime.UtcNow;

        var statement = new BankStatement
        {
            BankStatementId = Guid.NewGuid().ToString(),
            BankAccountId = bankAccountId,
            StatementStartDate = startDate,
            StatementEndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            StatementNumber = statementNumber,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _bankRepository.CreateStatementAsync(statement);

        return statement.BankStatementId;
    }

    public async Task<string> CreateTransactionAsync(
        string bankAccountId,
        string? bankStatementId,
        DateTime transactionDate,
        string? description,
        decimal moneyIn,
        decimal moneyOut,
        decimal? balanceAfterTransaction,
        string? reference)
    {
        if (moneyIn > 0 && moneyOut > 0)
            throw new InvalidOperationException("Only MoneyIn or MoneyOut may be greater than zero.");

        var now = DateTime.UtcNow;

        var transaction = new BankTransaction
        {
            BankTransactionId = Guid.NewGuid().ToString(),
            BankAccountId = bankAccountId,
            BankStatementId = bankStatementId,
            TransactionDate = transactionDate,
            Description = description,
            MoneyIn = moneyIn,
            MoneyOut = moneyOut,
            BalanceAfterTransaction = balanceAfterTransaction,
            Reference = reference,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _bankRepository.CreateTransactionAsync(transaction);

        return transaction.BankTransactionId;
    }

    public Task<IReadOnlyList<BankTransaction>> ListTransactionsAsync()
    {
        return _bankRepository.ListTransactionsAsync();
    }

    public Task MatchExpenseAsync(string bankTransactionId, string ExpenseId)
    {
        return _bankRepository.MatchExpenseAsync(bankTransactionId, ExpenseId);
    }

    public Task UnmatchExpenseAsync(string bankTransactionId)
    {
        return _bankRepository.UnmatchExpenseAsync(bankTransactionId);
    }
}