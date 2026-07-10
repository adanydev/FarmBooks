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

    private void ExpenseEditor_ExpenseSaved(object? sender, ExpenseDetailsViewModel details)
    {
        if (SelectedExpense is null)
            return;

        SelectedExpense.ExpenseDate = details.ExpenseDate;
        SelectedExpense.PaidDate = details.PaidDate;
        SelectedExpense.SourceType = details.SourceType;
        SelectedExpense.DocumentNumber = details.DocumentNumber;
        SelectedExpense.BusinessName = details.BusinessName;
        SelectedExpense.Description = details.Description;
        SelectedExpense.Total = details.Total;
        SelectedExpense.Status = details.Status;
    }
}
