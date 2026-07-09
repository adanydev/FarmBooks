using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseLineItemViewModel : ViewModelBase
{
    private string _description = "";
    private string _accountingCode = "";
    private decimal _amount;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string AccountingCode
    {
        get => _accountingCode;
        set => SetProperty(ref _accountingCode, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }
}