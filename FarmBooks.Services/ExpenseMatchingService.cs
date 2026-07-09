using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class ExpenseMatchingService
{
    private readonly ExpenseMatchRepository _matches;

    public ExpenseMatchingService(ExpenseMatchRepository matches)
    {
        _matches = matches;
    }

    public async Task<string> MatchAsync(
        string expenseId,
        string bankTransactionId,
        string? notes = null)
    {
        var now = DateTime.UtcNow;

        var match = new ExpenseMatch
        {
            ExpenseMatchId = Guid.NewGuid().ToString(),
            ExpenseId = expenseId,
            BankTransactionId = bankTransactionId,
            MatchedAt = now,
            Notes = notes,
            CreatedAt = now
        };

        await _matches.CreateAsync(match);

        return match.ExpenseMatchId;
    }

    public Task UnmatchByBankTransactionAsync(string bankTransactionId)
    {
        return _matches.SoftDeleteByBankTransactionAsync(bankTransactionId);
    }

    public Task<IReadOnlyList<ExpenseMatch>> ListActiveMatchesAsync()
    {
        return _matches.ListActiveAsync();
    }

    public Task<bool> IsBankTransactionMatchedAsync(string bankTransactionId)
    {
        return _matches.IsBankTransactionMatchedAsync(bankTransactionId);
    }

    public Task<bool> IsExpenseMatchedAsync(string expenseId)
    {
        return _matches.IsExpenseMatchedAsync(expenseId);
    }
}