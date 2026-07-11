using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseListViewModel : ViewModelBase
{
    private readonly IExpenseService _expenseService;
    private ExpenseListFilter _selectedFilter = ExpenseListFilter.All;
    private string _searchText = "";

    public ExpenseListViewModel(IExpenseService expenseService)
    {
        _expenseService = expenseService;

        FilteredExpenses = CollectionViewSource.GetDefaultView(Expenses);

        FilteredExpenses.Filter = FilterExpense;
    }

    public ObservableCollection<ExpenseListRowViewModel> Expenses { get; } = new();
    public ICollectionView FilteredExpenses { get; }

    public IReadOnlyList<ExpenseListFilterOption> FilterOptions { get; } =
    [
        new(ExpenseListFilter.All, "All Expenses"),
        new(ExpenseListFilter.VatNeedsAttention, "VAT Needs Attention"),
        new(ExpenseListFilter.VatReady, "VAT Ready"),
        new(ExpenseListFilter.TaxNeedsAttention, "Tax Needs Attention"),
        new(ExpenseListFilter.TaxReady, "Tax Ready"),
    ];

    public ExpenseListFilter SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                FilteredExpenses.Refresh();
                OnPropertyChanged(nameof(VisibleExpenseCount));
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilteredExpenses.Refresh();
                OnPropertyChanged(nameof(VisibleExpenseCount));
            }
        }
    }

    public int VisibleExpenseCount => FilteredExpenses.Cast<object>().Count();

    private bool FilterExpense(object item)
    {
        if (item is not ExpenseListRowViewModel expense)
            return false;

        if (!MatchesWorkflowFilter(expense))
            return false;

        return MatchesSearch(expense);
    }

    private bool MatchesWorkflowFilter(ExpenseListRowViewModel expense)
    {
        return SelectedFilter switch
        {
            ExpenseListFilter.All => true,

            ExpenseListFilter.VatNeedsAttention => !expense.IsVatReady,

            ExpenseListFilter.VatReady => expense.IsVatReady,

            ExpenseListFilter.TaxNeedsAttention => !expense.IsTaxReady,

            ExpenseListFilter.TaxReady => expense.IsTaxReady,

            _ => true,
        };
    }

    private bool MatchesSearch(ExpenseListRowViewModel expense)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var search = SearchText.Trim();

        return Contains(expense.BusinessName, search)
            || Contains(expense.DocumentNumber, search)
            || Contains(expense.Description, search);
    }

    private static bool Contains(string? value, string search)
    {
        return value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
    }

    public async Task LoadAsync()
    {
        var expenses = await _expenseService.GetExpenseListAsync();

        Expenses.Clear();

        foreach (var expense in expenses)
        {
            Expenses.Add(MapToRow(expense));
        }

        FilteredExpenses.Refresh();
        OnPropertyChanged(nameof(VisibleExpenseCount));
    }

    public async Task<ExpenseListRowViewModel> CreateNewExpenseAsync()
    {
        var defaultDate = DateTime.Today;

        var expenseId = await _expenseService.CreateExpenseAsync(
            expenseDate: defaultDate,
            paidDate: defaultDate,
            sourceType: ExpenseSourceType.Receipt,
            documentNumber: null,
            businessName: null,
            description: null,
            total: 0m,
            notes: null
        );

        var row = new ExpenseListRowViewModel
        {
            ExpenseId = expenseId,
            ExpenseDate = defaultDate,
            PaidDate = defaultDate,
            SourceType = "Receipt",
            BusinessName = "",
            DocumentNumber = "",
            Description = "",
            Total = 0m,
            Status = "Needs Review",
            Matched = false,
            LineItemCount = 0,
            DocumentCount = 0,
        };

        Expenses.Insert(0, row);

        FilteredExpenses.Refresh();
        OnPropertyChanged(nameof(VisibleExpenseCount));

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
            FilteredExpenses.Refresh();
            OnPropertyChanged(nameof(VisibleExpenseCount));
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

            IsVatReady = expense.IsVatReady,
            IsTaxReady = expense.IsTaxReady,
            VatIssueCount = expense.VatIssueCount,
            TaxIssueCount = expense.TaxIssueCount,

            VatIssuesToolTip =
                expense.VatIssues.Count == 0
                    ? "VAT is ready."
                    : string.Join(
                        Environment.NewLine,
                        expense.VatIssues.Select(issue => $"• {issue.Message}")
                    ),
            TaxIssuesToolTip =
                expense.TaxIssues.Count == 0
                    ? "Tax work is ready."
                    : string.Join(
                        Environment.NewLine,
                        expense.TaxIssues.Select(issue => $"• {issue.Message}")
                    ),
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

        destination.IsVatReady = source.IsVatReady;
        destination.IsTaxReady = source.IsTaxReady;
        destination.VatIssueCount = source.VatIssueCount;
        destination.TaxIssueCount = source.TaxIssueCount;

        destination.VatIssuesToolTip = source.VatIssuesToolTip;
        destination.TaxIssuesToolTip = source.TaxIssuesToolTip;
    }
}
