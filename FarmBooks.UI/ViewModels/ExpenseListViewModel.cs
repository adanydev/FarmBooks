using System.Collections.ObjectModel;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseListViewModel : ViewModelBase
{
    public ObservableCollection<ExpenseGridRowViewModel> Expenses { get; } = new();

    private ExpenseGridRowViewModel? _selectedExpense;

    public ExpenseGridRowViewModel? SelectedExpense
    {
        get => _selectedExpense;
        set => SetProperty(ref _selectedExpense, value);
    }

    public void LoadSampleData()
    {
        Expenses.Clear();

        var tractorSupply = new ExpenseGridRowViewModel
        {
            ExpenseDate = DateTime.Today,
            PaidDate = DateTime.Today,
            SourceType = "Receipt",
            BusinessName = "Tractor Supply",
            DocumentNumber = "1001",
            Description = "Feed and supplies",
            Total = 128.45m,
            Matched = false,
            DocumentCount = 1
        };

        tractorSupply.LineItems.Add(new ExpenseLineItemViewModel
        {
            Description = "Feed",
            AccountingCode = "Feed",
            Amount = 100.00m
        });

        tractorSupply.LineItems.Add(new ExpenseLineItemViewModel
        {
            Description = "Supplies",
            AccountingCode = "Supplies",
            Amount = 28.45m
        });

        Expenses.Add(tractorSupply);

        Expenses.Add(new ExpenseGridRowViewModel
        {
            ExpenseDate = DateTime.Today.AddDays(-2),
            PaidDate = null,
            SourceType = "Manual",
            BusinessName = "Local Hardware",
            DocumentNumber = "",
            Description = "Fence repair materials",
            Total = 74.20m,
            Matched = false,
            DocumentCount = 0
        });

        SelectedExpense = Expenses.FirstOrDefault();
    }

    public ExpenseGridRowViewModel CreateNewExpense()
    {
        var expense = new ExpenseGridRowViewModel
        {
            ExpenseDate = DateTime.Today,
            PaidDate = DateTime.Today,
            SourceType = "Receipt",
            BusinessName = "",
            DocumentNumber = "",
            Description = "",
            Total = 0m,
            Matched = false,
            DocumentCount = 0
        };

        Expenses.Insert(0, expense);
        SelectedExpense = expense;

        return expense;
    }
}