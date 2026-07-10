using System.Collections.ObjectModel;
using System.Windows.Input;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseLineItemsViewModel : ViewModelBase
{
    public ExpenseLineItemsViewModel()
    {
        AddLineItemCommand = new RelayCommand(AddLineItem);
    }

    public ObservableCollection<ExpenseLineItemViewModel> LineItems { get; } = new();

    public IReadOnlyList<string> AccountingCodes { get; } =
    ["Feed", "Supplies", "Repairs", "Fuel", "Seed", "Utilities", "Equipment"];

    public ICommand AddLineItemCommand { get; }

    private void AddLineItem()
    {
        LineItems.Add(
            new ExpenseLineItemViewModel
            {
                AccountingCode = "",
                Description = "",
                Amount = 0m,
            }
        );
    }
}
