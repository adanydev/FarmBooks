using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface IExpenseLineItemService
{
    Task<string> AddLineItemAsync(
        string expenseId,
        string? codeId,
        string? description,
        decimal total,
        string? vatTreatment = null
    );

    Task UpdateLineItemAsync(
        string expenseLineItemId,
        string? codeId,
        string? description,
        decimal total,
        string? vatTreatment = null
    );

    Task<IReadOnlyList<ExpenseLineItem>> ListForExpenseAsync(string expenseId);

    Task DeleteLineItemAsync(string expenseLineItemId);
}
