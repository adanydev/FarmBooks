using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class TransactionLineItemService : ITransactionLineItemService
{
    private readonly TransactionLineItemRepository _lineItems;

    public TransactionLineItemService(TransactionLineItemRepository lineItems)
    {
        _lineItems = lineItems;
    }

    public async Task<string> AddLineItemAsync(
        string transactionId,
        string? codeId,
        string? description,
        decimal total,
        int statementOrder
    )
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new InvalidOperationException("Transaction ID is required.");
        }

        if (total == 0)
        {
            throw new InvalidOperationException("Line item total cannot be zero.");
        }

        var now = DateTime.UtcNow;

        var item = new TransactionLineItem
        {
            TransactionLineItemId = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            CodeId = string.IsNullOrWhiteSpace(codeId) ? null : codeId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Total = total,
            StatementOrder = statementOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _lineItems.AddAsync(item);

        return item.TransactionLineItemId;
    }

    public async Task<IReadOnlyList<TransactionLineItem>> ListForTransactionAsync(
        string transactionId
    )
    {
        return await _lineItems.ListForTransactionAsync(transactionId);
    }

    public Task DeleteLineItemAsync(string transactionLineItemId)
    {
        return _lineItems.SoftDeleteAsync(transactionLineItemId);
    }

    public async Task UpdateLineItemAsync(
        string transactionLineItemId,
        string? codeId,
        string? description,
        decimal total,
        int statementOrder
    )
    {
        if (string.IsNullOrWhiteSpace(transactionLineItemId))
        {
            throw new InvalidOperationException("Transaction line item ID is required.");
        }

        if (total == 0)
        {
            throw new InvalidOperationException("Line item total cannot be zero.");
        }

        var item = new TransactionLineItem
        {
            TransactionLineItemId = transactionLineItemId,
            CodeId = string.IsNullOrWhiteSpace(codeId) ? null : codeId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Total = total,
            StatementOrder = statementOrder,
            UpdatedAt = DateTime.UtcNow,
        };

        await _lineItems.UpdateAsync(item);
    }
}
