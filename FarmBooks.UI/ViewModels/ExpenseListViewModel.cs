using System.Collections.ObjectModel;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseListViewModel : ViewModelBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseListViewModel(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    public ObservableCollection<ExpenseListRowViewModel> Expenses { get; } = new();

    public async Task LoadAsync()
    {
        Expenses.Clear();

        var expenses = await _expenseService.GetExpenseListAsync();

        foreach (var expense in expenses)
        {
            Expenses.Add(MapToRow(expense));
        }
    }

    public async Task<ExpenseListRowViewModel> CreateNewExpenseAsync()
    {
        var expenseId = await _expenseService.CreateExpenseAsync(
            expenseDate: DateTime.Today,
            paidDate: DateTime.Today,
            sourceType: FarmBooks.Core.Models.ExpenseSourceType.Receipt,
            documentNumber: null,
            businessName: null,
            description: null,
            total: 0m,
            notes: null
        );

        var row = new ExpenseListRowViewModel
        {
            ExpenseId = expenseId,
            ExpenseDate = DateTime.Today,
            PaidDate = DateTime.Today,
            SourceType = "Receipt",
            BusinessName = "",
            DocumentNumber = "",
            Description = "",
            Total = 0m,
            Status = "Needs Line Items",
            Matched = false,
            LineItemCount = 0,
            DocumentCount = 0,
        };

        Expenses.Insert(0, row);

        return row;
    }

    private static ExpenseListRowViewModel MapToRow(ExpenseListItemDto expense)
    {
        return new ExpenseListRowViewModel
        {
            ExpenseId = expense.ExpenseId,
            ExpenseDate = expense.ExpenseDate,
            PaidDate = expense.PaidDate,
            SourceType = expense.SourceType,
            DocumentNumber = expense.DocumentNumber ?? "",
            BusinessName = expense.BusinessName ?? "",
            Description = expense.Description ?? "",
            Total = expense.Total,
            Status = expense.Status,
            Matched = expense.IsMatched,
            LineItemCount = expense.LineItemCount,
            DocumentCount = expense.DocumentCount,
        };
    }
}
