using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FarmBooks.UI.Infrastructure;
using System.Windows.Input;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseGridRowViewModel : ViewModelBase
{
    private DateTime _expenseDate;
    private DateTime? _paidDate;
    private string _sourceType = "";
    private string _businessName = "";
    private string _documentNumber = "";
    private string _description = "";
    private decimal _total;
    private string _status = "";
    private bool _matched;
    private int _lineItemCount;
    private int _documentCount;
    public ICommand AddLineItemCommand { get; }

    public IReadOnlyList<string> AccountingCodes { get; } =
    [
        "Feed",
        "Supplies",
        "Repairs",
        "Fuel",
        "Seed",
        "Utilities",
        "Equipment"
    ];

    public ExpenseGridRowViewModel()
    {
        AddLineItemCommand = new RelayCommand(AddLineItem);
        LineItems.CollectionChanged += LineItems_CollectionChanged;
    }

    public ObservableCollection<ExpenseLineItemViewModel> LineItems { get; } = new();

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

    public string BusinessName
    {
        get => _businessName;
        set => SetProperty(ref _businessName, value);
    }

    public string DocumentNumber
    {
        get => _documentNumber;
        set => SetProperty(ref _documentNumber, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal Total
    {
        get => _total;
        set
        {
            if (SetProperty(ref _total, value))
                RefreshCalculatedFields();
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool Matched
    {
        get => _matched;
        set => SetProperty(ref _matched, value);
    }

    public int LineItemCount
    {
        get => _lineItemCount;
        private set => SetProperty(ref _lineItemCount, value);
    }

    public int DocumentCount
    {
        get => _documentCount;
        set => SetProperty(ref _documentCount, value);
    }

    private void LineItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ExpenseLineItemViewModel item in e.NewItems)
                item.PropertyChanged += LineItem_PropertyChanged;
        }

        if (e.OldItems is not null)
        {
            foreach (ExpenseLineItemViewModel item in e.OldItems)
                item.PropertyChanged -= LineItem_PropertyChanged;
        }

        RefreshCalculatedFields();
    }

    private void LineItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ExpenseLineItemViewModel.Amount) or nameof(ExpenseLineItemViewModel.AccountingCode))
            RefreshCalculatedFields();
    }

    private void RefreshCalculatedFields()
    {
        LineItemCount = LineItems.Count;

        if (LineItems.Count == 0)
        {
            Status = "Needs Line Items";
            return;
        }

        if (LineItems.Any(x => string.IsNullOrWhiteSpace(x.AccountingCode)))
        {
            Status = "Needs Accounting Code";
            return;
        }

        var lineTotal = LineItems.Sum(x => x.Amount);

        if (lineTotal != Total)
        {
            Status = "Line Total Mismatch";
            return;
        }

        Status = "Complete";
    }

    private void AddLineItem()
    {
        LineItems.Add(new ExpenseLineItemViewModel
        {
            Description = "",
            AccountingCode = "",
            Amount = 0m
        });
    }
    
}