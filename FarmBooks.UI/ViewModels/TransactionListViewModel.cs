using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionListViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;

    private TransactionListFilter _selectedFilter = TransactionListFilter.All;

    private string _searchText = "";

    public TransactionListViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;

        FilteredTransactions = CollectionViewSource.GetDefaultView(Transactions);

        FilteredTransactions.Filter = FilterTransaction;
    }

    public ObservableCollection<TransactionListRowViewModel> Transactions { get; } = new();

    public ICollectionView FilteredTransactions { get; }

    public IReadOnlyList<TransactionListFilterOption> FilterOptions { get; } =
    [
        new(TransactionListFilter.All, "All Transactions"),
        new(TransactionListFilter.VatNeedsAttention, "VAT Needs Attention"),
        new(TransactionListFilter.VatReady, "VAT Ready"),
        new(TransactionListFilter.TaxNeedsAttention, "Tax Needs Attention"),
        new(TransactionListFilter.TaxReady, "Tax Ready"),
    ];

    public TransactionListFilter SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                RefreshView();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshView();
            }
        }
    }

    public int VisibleTransactionCount => FilteredTransactions.Cast<object>().Count();

    public async Task LoadAsync()
    {
        var transactions = await _transactionService.GetTransactionListAsync();

        Transactions.Clear();

        foreach (var transaction in transactions)
        {
            Transactions.Add(MapToRow(transaction));
        }

        RefreshView();
    }

    public Task ReloadAsync()
    {
        return LoadAsync();
    }

    public async Task<TransactionListRowViewModel> CreateNewTransactionAsync(
        DateTime? defaultPaymentDate,
        string? insertAfterTransactionId
    )
    {
        var transactionId = await _transactionService.CreateTransactionAsync(
            receiptDate: null,
            paymentDate: defaultPaymentDate?.Date,
            sourceType: TransactionSourceType.Receipt,
            documentNumber: null,
            businessName: null,
            description: null,
            total: 0m,
            notes: null,
            insertAfterTransactionId: insertAfterTransactionId
        );

        // Existing rows may have shifted.
        await LoadAsync();

        return Transactions.First(transaction => transaction.TransactionId == transactionId);
    }

    public async Task<TransactionListRowViewModel?> RefreshTransactionAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return null;
        }

        var transactions = await _transactionService.GetTransactionListAsync();

        var refreshedTransaction = transactions.FirstOrDefault(transaction =>
            transaction.TransactionId == transactionId
        );

        if (refreshedTransaction is null)
            return null;

        var refreshedRow = MapToRow(refreshedTransaction);

        var existingRow = Transactions.FirstOrDefault(transaction =>
            transaction.TransactionId == transactionId
        );

        if (existingRow is null)
        {
            Transactions.Insert(0, refreshedRow);
            RefreshView();
            return refreshedRow;
        }

        CopyValues(refreshedRow, existingRow);

        RefreshView();

        return existingRow;
    }

    public void RemoveTransaction(string transactionId)
    {
        var transaction = Transactions.FirstOrDefault(item => item.TransactionId == transactionId);

        if (transaction is null)
            return;

        Transactions.Remove(transaction);
        RefreshView();
    }

    private bool FilterTransaction(object item)
    {
        if (item is not TransactionListRowViewModel transaction)
        {
            return false;
        }

        return MatchesWorkflowFilter(transaction) && MatchesSearch(transaction);
    }

    private bool MatchesWorkflowFilter(TransactionListRowViewModel transaction)
    {
        return SelectedFilter switch
        {
            TransactionListFilter.All => true,

            TransactionListFilter.VatNeedsAttention => !transaction.IsVatReady,

            TransactionListFilter.VatReady => transaction.IsVatReady,

            TransactionListFilter.TaxNeedsAttention => !transaction.IsTaxReady,

            TransactionListFilter.TaxReady => transaction.IsTaxReady,

            _ => true,
        };
    }

    private bool MatchesSearch(TransactionListRowViewModel transaction)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var search = SearchText.Trim();

        return Contains(transaction.BusinessName, search)
            || Contains(transaction.DocumentNumber, search)
            || Contains(transaction.Description, search);
    }

    private static bool Contains(string? value, string search)
    {
        return value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
    }

    private void RefreshView()
    {
        FilteredTransactions.Refresh();

        OnPropertyChanged(nameof(VisibleTransactionCount));
    }

    private static TransactionListRowViewModel MapToRow(TransactionListItemDto transaction)
    {
        return new TransactionListRowViewModel
        {
            TransactionId = transaction.TransactionId,

            ReceiptDate = transaction.ReceiptDate,

            PaymentDate = transaction.PaymentDate,

            SourceType = transaction.SourceType,

            DocumentNumber = transaction.DocumentNumber ?? "",

            BusinessName = transaction.BusinessName ?? "",

            Description = transaction.Description ?? "",

            Total = transaction.Total,
            Status = transaction.Status,
            Matched = transaction.IsMatched,

            LineItemCount = transaction.LineItemCount,

            DocumentCount = transaction.DocumentCount,

            StatementOrder = transaction.StatementOrder,

            IsVatReady = transaction.IsVatReady,

            IsTaxReady = transaction.IsTaxReady,

            VatIssueCount = transaction.VatIssueCount,

            TaxIssueCount = transaction.TaxIssueCount,

            VatIssuesToolTip = CreateIssuesToolTip(transaction.VatIssues, "VAT is ready."),

            TaxIssuesToolTip = CreateIssuesToolTip(transaction.TaxIssues, "Tax work is ready."),
        };
    }

    private static string CreateIssuesToolTip(
        IReadOnlyList<TransactionWorkflowIssueDto> issues,
        string readyMessage
    )
    {
        return issues.Count == 0
            ? readyMessage
            : string.Join(Environment.NewLine, issues.Select(issue => $"• {issue.Message}"));
    }

    private static void CopyValues(
        TransactionListRowViewModel source,
        TransactionListRowViewModel destination
    )
    {
        destination.ReceiptDate = source.ReceiptDate;

        destination.PaymentDate = source.PaymentDate;

        destination.SourceType = source.SourceType;

        destination.DocumentNumber = source.DocumentNumber;

        destination.BusinessName = source.BusinessName;

        destination.Description = source.Description;

        destination.Total = source.Total;

        destination.Status = source.Status;

        destination.Matched = source.Matched;

        destination.LineItemCount = source.LineItemCount;

        destination.DocumentCount = source.DocumentCount;

        destination.StatementOrder = source.StatementOrder;

        destination.IsVatReady = source.IsVatReady;

        destination.IsTaxReady = source.IsTaxReady;

        destination.VatIssueCount = source.VatIssueCount;

        destination.TaxIssueCount = source.TaxIssueCount;

        destination.VatIssuesToolTip = source.VatIssuesToolTip;

        destination.TaxIssuesToolTip = source.TaxIssuesToolTip;
    }
}
