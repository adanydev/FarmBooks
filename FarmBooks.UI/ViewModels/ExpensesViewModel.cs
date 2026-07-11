using System.Windows.Input;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpensesViewModel : ViewModelBase
{
    private ExpenseListRowViewModel? _selectedExpense;
    private bool _isInitialized;
    private bool _isLoading;
    private string _loadMessage = "";

    public ExpensesViewModel(ExpenseListViewModel expenseList, ExpenseEditorViewModel expenseEditor)
    {
        ExpenseList = expenseList;
        ExpenseEditor = expenseEditor;

        NewExpenseCommand = new RelayCommand(AddNewExpense);

        ExpenseEditor.ExpenseSaved += ExpenseEditor_ExpenseSaved;
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string LoadMessage
    {
        get => _loadMessage;
        private set => SetProperty(ref _loadMessage, value);
    }

    public ExpenseListViewModel ExpenseList { get; }
    public ExpenseEditorViewModel ExpenseEditor { get; }

    public ExpenseListRowViewModel? SelectedExpense
    {
        get => _selectedExpense;
        set
        {
            if (SetProperty(ref _selectedExpense, value))
            {
                _ = ExpenseEditor.LoadAsync(value);
            }
        }
    }

    public ICommand NewExpenseCommand { get; }

    public async Task InitializeAsync()
    {
        if (_isInitialized || IsLoading)
            return;

        IsLoading = true;
        LoadMessage = "Loading expenses...";

        try
        {
            await ExpenseList.LoadAsync();

            SelectedExpense = ExpenseList.Expenses.FirstOrDefault();

            _isInitialized = true;

            LoadMessage = ExpenseList.Expenses.Count == 0 ? "No expenses found." : "";
        }
        catch (Exception ex)
        {
            LoadMessage = $"Could not load expenses: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void AddNewExpense()
    {
        var row = await ExpenseList.CreateNewExpenseAsync();
        SelectedExpense = row;
    }

    private async void ExpenseEditor_ExpenseSaved(object? sender, string expenseId)
    {
        var refreshedRow = await ExpenseList.RefreshExpenseAsync(expenseId);

        if (refreshedRow is null)
            return;

        var isStillVisible = ExpenseList
            .FilteredExpenses.Cast<ExpenseListRowViewModel>()
            .Any(row => row.ExpenseId == expenseId);

        if (isStillVisible)
        {
            SelectedExpense = refreshedRow;
            return;
        }

        SelectedExpense = ExpenseList
            .FilteredExpenses.Cast<ExpenseListRowViewModel>()
            .FirstOrDefault();
    }
}
