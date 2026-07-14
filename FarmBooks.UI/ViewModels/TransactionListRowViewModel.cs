using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionListRowViewModel : ViewModelBase
{
    private string _transactionId = "";
    private DateTime? _receiptDate;
    private DateTime? _paymentDate;
    private string _sourceType = "";
    private string _documentNumber = "";
    private string _businessName = "";
    private string _description = "";
    private decimal _total;
    private string _status = "";
    private bool _matched;
    private int _lineItemCount;
    private int _documentCount;
    private bool _isVatReady;
    private bool _isTaxReady;

    private int _statementOrder;

    private int _vatIssueCount;
    private int _taxIssueCount;
    private string _vatIssuesToolTip = "";

    private string _taxIssuesToolTip = "";

    public string TaxIssuesToolTip
    {
        get => _taxIssuesToolTip;
        set => SetProperty(ref _taxIssuesToolTip, value);
    }

    public string VatIssuesToolTip
    {
        get => _vatIssuesToolTip;
        set => SetProperty(ref _vatIssuesToolTip, value);
    }

    public string TransactionId
    {
        get => _transactionId;
        set => SetProperty(ref _transactionId, value);
    }

    public DateTime? ReceiptDate
    {
        get => _receiptDate;
        set => SetProperty(ref _receiptDate, value);
    }

    public DateTime? PaymentDate
    {
        get => _paymentDate;
        set => SetProperty(ref _paymentDate, value);
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

    public bool IsVatReady
    {
        get => _isVatReady;
        set
        {
            if (SetProperty(ref _isVatReady, value))
            {
                OnPropertyChanged(nameof(VatStatusText));
            }
        }
    }

    public bool IsTaxReady
    {
        get => _isTaxReady;
        set
        {
            if (SetProperty(ref _isTaxReady, value))
            {
                OnPropertyChanged(nameof(TaxStatusText));
            }
        }
    }

    public int VatIssueCount
    {
        get => _vatIssueCount;
        set
        {
            if (SetProperty(ref _vatIssueCount, value))
            {
                OnPropertyChanged(nameof(VatStatusText));
            }
        }
    }

    public int TaxIssueCount
    {
        get => _taxIssueCount;
        set
        {
            if (SetProperty(ref _taxIssueCount, value))
            {
                OnPropertyChanged(nameof(TaxStatusText));
            }
        }
    }

    public int StatementOrder
    {
        get => _statementOrder;
        set => SetProperty(ref _statementOrder, value);
    }

    public string VatStatusText => IsVatReady ? "✓" : $"{VatIssueCount}";

    public string TaxStatusText => IsTaxReady ? "✓" : $"{TaxIssueCount}";
}
