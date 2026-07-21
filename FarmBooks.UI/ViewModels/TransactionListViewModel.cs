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
        FilteredTransactions.SortDescriptions.Add(
            new SortDescription(
                nameof(TransactionListRowViewModel.PaymentDate),
                ListSortDirection.Descending
            )
        );
        FilteredTransactions.SortDescriptions.Add(
            new SortDescription(
                nameof(TransactionListRowViewModel.StatementOrder),
                ListSortDirection.Ascending
            )
        );
    }

    public ObservableCollection<TransactionListRowViewModel> Transactions { get; } = [];

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
            foreach (var row in MapToRows(transaction))
            {
                Transactions.Add(row);
            }
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

        await LoadAsync();

        return Transactions.First(transaction => transaction.TransactionId == transactionId);
    }

    public async Task<TransactionListRowViewModel?> RefreshTransactionAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return null;
        }

        await LoadAsync();

        return Transactions.FirstOrDefault(transaction =>
            transaction.TransactionId == transactionId
        );
    }

    public void RemoveTransaction(string transactionId)
    {
        var rows = Transactions.Where(item => item.TransactionId == transactionId).ToList();

        foreach (var row in rows)
        {
            Transactions.Remove(row);
        }

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
            || Contains(transaction.Description, search)
            || Contains(transaction.Code, search)
            || Contains(transaction.CodeName, search)
            || Contains(transaction.LineItemDescription, search);
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

    private static IEnumerable<TransactionListRowViewModel> MapToRows(
        TransactionListItemDto transaction
    )
    {
        if (transaction.LineItems.Count == 0)
        {
            yield return MapToRow(transaction, null);
            yield break;
        }

        foreach (var lineItem in transaction.LineItems)
        {
            yield return MapToRow(transaction, lineItem);
        }
    }

    private static TransactionListRowViewModel MapToRow(
        TransactionListItemDto transaction,
        TransactionLineItemDto? lineItem
    )
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

            CodesSummary = transaction.CodesSummary,

            CodesToolTip = transaction.CodesToolTip,

            TransactionLineItemId = lineItem?.TransactionLineItemId ?? "",

            Code = string.IsNullOrWhiteSpace(lineItem?.Code) ? "Uncoded" : lineItem.Code,

            CodeName = lineItem?.CodeName ?? "",

            LineItemDescription = lineItem?.Description ?? "",

            LineItemTotal = lineItem?.Total ?? 0m,

            AllocatedTotal = transaction.AllocatedTotal,

            RemainingTotal = transaction.RemainingTotal,

            LineItems = transaction.LineItems,
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

}
