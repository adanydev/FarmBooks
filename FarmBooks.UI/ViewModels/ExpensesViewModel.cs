using System.Windows.Input;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpensesViewModel : ViewModelBase
{
    private ExpenseListRowViewModel? _selectedExpense;

    public ExpensesViewModel(ExpenseListViewModel expenseList, ExpenseEditorViewModel expenseEditor)
    {
        ExpenseList = expenseList;
        ExpenseEditor = expenseEditor;

        NewExpenseCommand = new RelayCommand(AddNewExpense);

        _ = LoadAsync();
        ExpenseEditor.ExpenseSaved += ExpenseEditor_ExpenseSaved;
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

    private async Task LoadAsync()
    {
        await ExpenseList.LoadAsync();
        SelectedExpense = ExpenseList.Expenses.FirstOrDefault();
    }

    private async void AddNewExpense()
    {
        var row = await ExpenseList.CreateNewExpenseAsync();
        SelectedExpense = row;
    }

    private async void ExpenseEditor_ExpenseSaved(object? sender, string expenseId)
    {
        var refreshedRow = await ExpenseList.RefreshExpenseAsync(expenseId);

        if (refreshedRow is not null)
        {
            SelectedExpense = refreshedRow;
        }
    }
}
