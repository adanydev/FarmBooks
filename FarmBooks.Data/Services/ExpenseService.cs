using FarmBooks.Core.Models;
using FarmBooks.Data.Services;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Data.Services;

public sealed class ExpenseService
{
    private readonly ExpenseRepository _expenses;
    private readonly ExpenseLineItemRepository _lineItems;

    public ExpenseService(
        ExpenseRepository expenses,
        ExpenseLineItemRepository lineItems)
    {
        _expenses = expenses;
        _lineItems = lineItems;
    }

    public async Task<string> CreateExpenseAsync(
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes)
    {
        if (expenseDate == default)
            throw new InvalidOperationException("Expense date is required.");

        if (total < 0)
            throw new InvalidOperationException("Expense total cannot be negative.");

        var now = DateTime.UtcNow;

        var expense = new Expense
        {
            ExpenseId = Guid.NewGuid().ToString(),
            ExpenseDate = expenseDate,
            PaidDate = paidDate,
            SourceType = sourceType,
            DocumentNumber = documentNumber,
            BusinessName = businessName,
            Description = description,
            Total = total,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _expenses.CreateAsync(expense);

        return expense.ExpenseId;
    }

    public async Task<ExpenseStatus> GetStatusAsync(string expenseId)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        var lineItems = await _lineItems.ListForExpenseAsync(expenseId);

        return ExpenseStatusCalculator.Calculate(expense, lineItems);
    }

    public Task<IReadOnlyList<Expense>> ListExpensesAsync()
    {
        return _expenses.ListAsync();
    }

    public async Task UpdateExpenseAsync(
        string expenseId,
        DateTime expenseDate,
        DateTime? paidDate,
        ExpenseSourceType sourceType,
        string? documentNumber,
        string? businessName,
        string? description,
        decimal total,
        string? notes)
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        if (total < 0)
            throw new InvalidOperationException("Expense total cannot be negative.");

        expense.ExpenseDate = expenseDate;
        expense.PaidDate = paidDate;
        expense.SourceType = sourceType;
        expense.DocumentNumber = documentNumber;
        expense.BusinessName = businessName;
        expense.Description = description;
        expense.Total = total;
        expense.Notes = notes;

        await _expenses.UpdateAsync(expense);
    }

    public Task DeleteExpenseAsync(string expenseId)
    {
        return _expenses.SoftDeleteAsync(expenseId);
    }

    public Task RestoreExpenseAsync(string expenseId)
    {
        return _expenses.RestoreAsync(expenseId);
    }
}