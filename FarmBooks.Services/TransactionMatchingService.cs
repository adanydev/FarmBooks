using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class TransactionMatchingService
{
    private readonly TransactionMatchRepository _matches;

    public TransactionMatchingService(TransactionMatchRepository matches)
    {
        _matches = matches;
    }

    public async Task<string> MatchAsync(
        string transactionId,
        string bankTransactionId,
        string? notes = null
    )
    {
        var now = DateTime.UtcNow;

        var match = new TransactionMatch
        {
            TransactionMatchId = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            BankTransactionId = bankTransactionId,
            MatchedAt = now,
            Notes = notes,
            CreatedAt = now,
        };

        await _matches.CreateAsync(match);

        return match.TransactionMatchId;
    }

    public Task UnmatchByBankTransactionAsync(string bankTransactionId)
    {
        return _matches.SoftDeleteByBankTransactionAsync(bankTransactionId);
    }

    public Task<IReadOnlyList<TransactionMatch>> ListActiveMatchesAsync()
    {
        return _matches.ListActiveAsync();
    }

    public Task<bool> IsBankTransactionMatchedAsync(string bankTransactionId)
    {
        return _matches.IsBankTransactionMatchedAsync(bankTransactionId);
    }

    public Task<bool> IsTransactionMatchedAsync(string transactionId)
    {
        return _matches.IsTransactionMatchedAsync(transactionId);
    }
}
