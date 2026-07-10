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

    public async Task<ExpenseListRowViewModel?> RefreshExpenseAsync(string expenseId)
    {
        if (string.IsNullOrWhiteSpace(expenseId))
            return null;

        var expenses = await _expenseService.GetExpenseListAsync();

        var refreshedExpense = expenses.FirstOrDefault(expense => expense.ExpenseId == expenseId);

        if (refreshedExpense is null)
            return null;

        var refreshedRow = MapToRow(refreshedExpense);

        var existingRow = Expenses.FirstOrDefault(expense => expense.ExpenseId == expenseId);

        if (existingRow is null)
        {
            Expenses.Insert(0, refreshedRow);
            return refreshedRow;
        }

        CopyValues(refreshedRow, existingRow);

        return existingRow;
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

    private static void CopyValues(
        ExpenseListRowViewModel source,
        ExpenseListRowViewModel destination
    )
    {
        destination.ExpenseDate = source.ExpenseDate;
        destination.PaidDate = source.PaidDate;
        destination.SourceType = source.SourceType;
        destination.DocumentNumber = source.DocumentNumber;
        destination.BusinessName = source.BusinessName;
        destination.Description = source.Description;
        destination.Total = source.Total;
        destination.Status = source.Status;
        destination.Matched = source.Matched;
        destination.LineItemCount = source.LineItemCount;
        destination.DocumentCount = source.DocumentCount;
    }
}
