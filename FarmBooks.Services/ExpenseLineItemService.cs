using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class ExpenseLineItemService : IExpenseLineItemService
{
    private readonly ExpenseLineItemRepository _lineItems;

    public ExpenseLineItemService(ExpenseLineItemRepository lineItems)
    {
        _lineItems = lineItems;
    }

    public async Task<string> AddLineItemAsync(
        string expenseId,
        string? codeId,
        string? description,
        decimal total,
        string? vatTreatment = null
    )
    {
        if (string.IsNullOrWhiteSpace(expenseId))
        {
            throw new InvalidOperationException("Expense ID is required.");
        }

        if (total < 0)
        {
            throw new InvalidOperationException("Line item total cannot be negative.");
        }

        var now = DateTime.UtcNow;

        var item = new ExpenseLineItem
        {
            ExpenseLineItemId = Guid.NewGuid().ToString(),
            ExpenseId = expenseId,
            CodeId = string.IsNullOrWhiteSpace(codeId) ? null : codeId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Total = total,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _lineItems.AddAsync(item);

        return item.ExpenseLineItemId;
    }

    public async Task<IReadOnlyList<ExpenseLineItem>> ListForExpenseAsync(string expenseId)
    {
        return await _lineItems.ListForExpenseAsync(expenseId);
    }

    public Task DeleteLineItemAsync(string expenseLineItemId)
    {
        return _lineItems.SoftDeleteAsync(expenseLineItemId);
    }

    public async Task UpdateLineItemAsync(
        string expenseLineItemId,
        string? codeId,
        string? description,
        decimal total,
        string? vatTreatment = null
    )
    {
        if (string.IsNullOrWhiteSpace(expenseLineItemId))
        {
            throw new InvalidOperationException("Expense line item ID is required.");
        }

        if (total < 0)
        {
            throw new InvalidOperationException("Line item total cannot be negative.");
        }

        var item = new ExpenseLineItem
        {
            ExpenseLineItemId = expenseLineItemId,
            CodeId = string.IsNullOrWhiteSpace(codeId) ? null : codeId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Total = total,
            UpdatedAt = DateTime.UtcNow,
        };

        await _lineItems.UpdateAsync(item);
    }
}
