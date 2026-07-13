using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface ITransactionLineItemService
{
    Task<string> AddLineItemAsync(
        string transactionId,
        string? codeId,
        string? description,
        decimal total
    );

    Task UpdateLineItemAsync(
        string transactionLineItemId,
        string? codeId,
        string? description,
        decimal total
    );

    Task<IReadOnlyList<TransactionLineItem>> ListForTransactionAsync(string transactionId);

    Task DeleteLineItemAsync(string transactionLineItemId);
}
