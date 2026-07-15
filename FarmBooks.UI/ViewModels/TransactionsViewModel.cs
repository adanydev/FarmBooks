using System.Windows.Input;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;
using FarmBooks.UI.Services;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionsViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;

    private readonly IConfirmationService _confirmationService;

    private TransactionListRowViewModel? _selectedTransaction;

    private bool _isInitialized;
    private bool _isLoading;
    private string _loadMessage = "";

    public TransactionsViewModel(
        TransactionListViewModel transactionList,
        TransactionEditorViewModel transactionEditor,
        ITransactionService transactionService,
        IConfirmationService confirmationService
    )
    {
        TransactionList = transactionList;
        TransactionEditor = transactionEditor;

        _transactionService = transactionService;
        _confirmationService = confirmationService;

        NewTransactionCommand = new RelayCommand(AddNewTransaction);

        MoveTransactionUpCommand = new AsyncRelayCommand(MoveSelectedTransactionUpAsync);

        MoveTransactionDownCommand = new AsyncRelayCommand(MoveSelectedTransactionDownAsync);

        DeleteTransactionCommand = new AsyncRelayCommand(DeleteSelectedTransactionAsync);

        TransactionEditor.TransactionSaved += TransactionEditor_TransactionSaved;
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string LoadMessage
    {
        get => _loadMessage;
        private set => SetProperty(ref _loadMessage, value);
    }

    public TransactionListViewModel TransactionList { get; }

    public TransactionEditorViewModel TransactionEditor { get; }

    public TransactionListRowViewModel? SelectedTransaction
    {
        get => _selectedTransaction;
        set
        {
            if (SetProperty(ref _selectedTransaction, value))
            {
                _ = TransactionEditor.LoadAsync(value);
            }
        }
    }

    public ICommand NewTransactionCommand { get; }

    public ICommand DeleteTransactionCommand { get; }

    public ICommand MoveTransactionUpCommand { get; }

    public ICommand MoveTransactionDownCommand { get; }

    public async Task InitializeAsync()
    {
        if (_isInitialized || IsLoading)
            return;

        IsLoading = true;
        LoadMessage = "Loading transactions...";

        try
        {
            await TransactionList.LoadAsync();

            SelectedTransaction = TransactionList.Transactions.FirstOrDefault();

            _isInitialized = true;

            LoadMessage = TransactionList.Transactions.Count == 0 ? "No transactions found." : "";
        }
        catch (Exception ex)
        {
            LoadMessage = $"Could not load transactions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void AddNewTransaction()
    {
        var selected = SelectedTransaction;

        try
        {
            LoadMessage = "Creating transaction...";

            var row = await TransactionList.CreateNewTransactionAsync(
                defaultPaymentDate: selected?.PaymentDate,
                insertAfterTransactionId: selected?.TransactionId
            );

            SelectedTransaction = row;
            LoadMessage = "";
        }
        catch (Exception ex)
        {
            LoadMessage = $"Could not create transaction: {ex.Message}";
        }
    }

    private async void TransactionEditor_TransactionSaved(object? sender, string transactionId)
    {
        var refreshedRow = await TransactionList.RefreshTransactionAsync(transactionId);

        if (refreshedRow is null)
            return;

        var isStillVisible = TransactionList
            .FilteredTransactions.Cast<TransactionListRowViewModel>()
            .Any(row => row.TransactionId == transactionId);

        SelectedTransaction = isStillVisible
            ? refreshedRow
            : TransactionList
                .FilteredTransactions.Cast<TransactionListRowViewModel>()
                .FirstOrDefault();
    }

    private async Task DeleteSelectedTransactionAsync()
    {
        var transaction = SelectedTransaction;

        if (transaction is null)
            return;

        var visibleBeforeDelete = TransactionList
            .FilteredTransactions.Cast<TransactionListRowViewModel>()
            .ToList();

        var deletedVisibleIndex = visibleBeforeDelete.FindIndex(row =>
            row.TransactionId == transaction.TransactionId
        );

        var businessName = string.IsNullOrWhiteSpace(transaction.BusinessName)
            ? "(No business name)"
            : transaction.BusinessName;

        var paymentDate = transaction.PaymentDate?.ToString("dd/MM/yyyy") ?? "(No payment date)";

        var confirmed = _confirmationService.Confirm(
            $"""
            Delete this transaction?

            Business: {businessName}
            Payment date: {paymentDate}
            Total: {transaction.Total:N2}

            The transaction will be removed from the transaction list.
            """,
            "Delete Transaction"
        );

        if (!confirmed)
            return;

        try
        {
            LoadMessage = "Deleting transaction...";

            await _transactionService.DeleteTransactionAsync(transaction.TransactionId);

            await TransactionList.ReloadAsync();

            var visibleAfterDelete = TransactionList
                .FilteredTransactions.Cast<TransactionListRowViewModel>()
                .ToList();

            if (visibleAfterDelete.Count == 0)
            {
                SelectedTransaction = null;
            }
            else
            {
                var nextIndex =
                    deletedVisibleIndex < 0
                        ? 0
                        : Math.Min(deletedVisibleIndex, visibleAfterDelete.Count - 1);

                SelectedTransaction = visibleAfterDelete[nextIndex];
            }

            LoadMessage = "";
        }
        catch (Exception ex)
        {
            LoadMessage = $"Could not delete transaction: {ex.Message}";
        }
    }

    private Task MoveSelectedTransactionUpAsync()
    {
        return MoveSelectedTransactionAsync(TransactionMoveDirection.Up);
    }

    private Task MoveSelectedTransactionDownAsync()
    {
        return MoveSelectedTransactionAsync(TransactionMoveDirection.Down);
    }

    private async Task MoveSelectedTransactionAsync(TransactionMoveDirection direction)
    {
        var selected = SelectedTransaction;

        if (selected is null)
            return;

        try
        {
            LoadMessage =
                direction == TransactionMoveDirection.Up
                    ? "Moving transaction up..."
                    : "Moving transaction down...";

            await _transactionService.MoveTransactionAsync(selected.TransactionId, direction);

            await TransactionList.ReloadAsync();

            SelectedTransaction = TransactionList.Transactions.FirstOrDefault(transaction =>
                transaction.TransactionId == selected.TransactionId
            );

            LoadMessage = "";
        }
        catch (Exception ex)
        {
            LoadMessage = $"Could not move transaction: {ex.Message}";
        }
    }
}
