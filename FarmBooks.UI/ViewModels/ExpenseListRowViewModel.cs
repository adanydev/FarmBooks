using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseListRowViewModel : ViewModelBase
{
    private string _expenseId = "";
    private DateTime _expenseDate;
    private DateTime? _paidDate;
    private string _sourceType = "";
    private string _documentNumber = "";
    private string _businessName = "";
    private string _description = "";
    private decimal _total;
    private string _status = "";
    private bool _matched;
    private int _lineItemCount;
    private int _documentCount;

    public string ExpenseId
    {
        get => _expenseId;
        set => SetProperty(ref _expenseId, value);
    }

    public DateTime ExpenseDate
    {
        get => _expenseDate;
        set => SetProperty(ref _expenseDate, value);
    }

    public DateTime? PaidDate
    {
        get => _paidDate;
        set => SetProperty(ref _paidDate, value);
    }

    public string SourceType
    {
        get => _sourceType;
        set => SetProperty(ref _sourceType, value);
    }

    public string DocumentNumber
    {
        get => _documentNumber;
        set => SetProperty(ref _documentNumber, value);
    }

    public string BusinessName
    {
        get => _businessName;
        set => SetProperty(ref _businessName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool Matched
    {
        get => _matched;
        set => SetProperty(ref _matched, value);
    }

    public int LineItemCount
    {
        get => _lineItemCount;
        set => SetProperty(ref _lineItemCount, value);
    }

    public int DocumentCount
    {
        get => _documentCount;
        set => SetProperty(ref _documentCount, value);
    }
}
