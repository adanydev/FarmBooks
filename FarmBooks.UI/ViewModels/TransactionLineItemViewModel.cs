using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionLineItemViewModel : ViewModelBase
{
    private string _transactionLineItemId = "";
    private string _codeId = "";
    private string _accountingCode = "";
    private string _description = "";
    private decimal _amount;

    public string TransactionLineItemId
    {
        get => _transactionLineItemId;
        set => SetProperty(ref _transactionLineItemId, value);
    }

    public string CodeId
    {
        get => _codeId;
        set => SetProperty(ref _codeId, value);
    }

    public string AccountingCode
    {
        get => _accountingCode;
        set => SetProperty(ref _accountingCode, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }
}
