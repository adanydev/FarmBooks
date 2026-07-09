using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class ExpenseLineItemService
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
        string? vatTreatment = null)
    {
        if (string.IsNullOrWhiteSpace(expenseId))
            throw new InvalidOperationException("Expense ID is required.");

        if (total < 0)
            throw new InvalidOperationException("Line item total cannot be negative.");

        var now = DateTime.UtcNow;

        var item = new ExpenseLineItem
        {
            ExpenseLineItemId = Guid.NewGuid().ToString(),
            ExpenseId = expenseId,
            CodeId = codeId,
            Description = description,
            Total = total,
            VATTreatment = vatTreatment,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _lineItems.AddAsync(item);

        return item.ExpenseLineItemId;
    }

    public Task<IReadOnlyList<ExpenseLineItem>> ListForExpenseAsync(string expenseId)
    {
        return _lineItems.ListForExpenseAsync(expenseId);
    }

    public Task DeleteLineItemAsync(string expenseLineItemId)
    {
        return _lineItems.SoftDeleteAsync(expenseLineItemId);
    }
}