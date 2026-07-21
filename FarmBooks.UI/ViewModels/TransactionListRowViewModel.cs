using FarmBooks.Core.DTOs.Transactions;
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

    private string _codesSummary = "Uncoded";
    private string _codesToolTip = "No accounting coding lines.";

    private string _transactionLineItemId = "";
    private string _code = "Uncoded";
    private string _codeName = "";
    private string _lineItemDescription = "";
    private decimal _lineItemTotal;

    private decimal _allocatedTotal;
    private decimal _remainingTotal;

    private IReadOnlyList<TransactionLineItemDto> _lineItems = [];

    private bool _isExpanded;

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
        set
        {
            if (SetProperty(ref _lineItemCount, value))
            {
                OnPropertyChanged(nameof(HasLineItems));
            }
        }
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

    public string VatIssuesToolTip
    {
        get => _vatIssuesToolTip;
        set => SetProperty(ref _vatIssuesToolTip, value);
    }

    public string TaxIssuesToolTip
    {
        get => _taxIssuesToolTip;
        set => SetProperty(ref _taxIssuesToolTip, value);
    }

    public string CodesSummary
    {
        get => _codesSummary;
        set => SetProperty(ref _codesSummary, value);
    }

    public string CodesToolTip
    {
        get => _codesToolTip;
        set => SetProperty(ref _codesToolTip, value);
    }

    public string TransactionLineItemId
    {
        get => _transactionLineItemId;
        set => SetProperty(ref _transactionLineItemId, value);
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string CodeName
    {
        get => _codeName;
        set => SetProperty(ref _codeName, value);
    }

    public string LineItemDescription
    {
        get => _lineItemDescription;
        set => SetProperty(ref _lineItemDescription, value);
    }

    public decimal LineItemTotal
    {
        get => _lineItemTotal;
        set => SetProperty(ref _lineItemTotal, value);
    }

    public decimal AllocatedTotal
    {
        get => _allocatedTotal;
        set
        {
            if (SetProperty(ref _allocatedTotal, value))
            {
                OnPropertyChanged(nameof(IsAllocationBalanced));
            }
        }
    }

    public decimal RemainingTotal
    {
        get => _remainingTotal;
        set
        {
            if (SetProperty(ref _remainingTotal, value))
            {
                OnPropertyChanged(nameof(IsAllocationBalanced));
            }
        }
    }

    public IReadOnlyList<TransactionLineItemDto> LineItems
    {
        get => _lineItems;
        set
        {
            if (SetProperty(ref _lineItems, value))
            {
                OnPropertyChanged(nameof(HasLineItems));
            }
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool HasLineItems => LineItems.Count > 0;

    public bool IsAllocationBalanced => Math.Abs(RemainingTotal) < 0.01m;

    public string VatStatusText => IsVatReady ? "✓" : $"{VatIssueCount}";

    public string TaxStatusText => IsTaxReady ? "✓" : $"{TaxIssueCount}";
}
