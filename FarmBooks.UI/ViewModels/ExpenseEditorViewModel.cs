using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseEditorViewModel : ViewModelBase
{
    private ExpenseGridRowViewModel? _selectedExpense;

    public ExpenseGridRowViewModel? SelectedExpense
    {
        get => _selectedExpense;
        set => SetProperty(ref _selectedExpense, value);
    }
}