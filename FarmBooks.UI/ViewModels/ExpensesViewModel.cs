using System.Windows.Input;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpensesViewModel : ViewModelBase
{
    public ExpenseListViewModel ExpenseList { get; } = new();
    public ExpenseEditorViewModel ExpenseEditor { get; } = new();

    public ICommand NewExpenseCommand { get; }

    public ExpensesViewModel()
    {
        NewExpenseCommand = new RelayCommand(AddNewExpense);

        ExpenseList.PropertyChanged += ExpenseList_PropertyChanged;

        ExpenseList.LoadSampleData();
        ExpenseEditor.SelectedExpense = ExpenseList.SelectedExpense;
    }

    private void AddNewExpense()
    {
        ExpenseEditor.SelectedExpense = ExpenseList.CreateNewExpense();
    }

    private void ExpenseList_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExpenseListViewModel.SelectedExpense))
        {
            ExpenseEditor.SelectedExpense = ExpenseList.SelectedExpense;
        }
    }
}