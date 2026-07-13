using System.Windows.Input;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionsViewModel : ViewModelBase
{
    private TransactionListRowViewModel? _selectedTransaction;
    private bool _isInitialized;
    private bool _isLoading;
    private string _loadMessage = "";

    public TransactionsViewModel(
        TransactionListViewModel transactionList,
        TransactionEditorViewModel transactionEditor
    )
    {
        TransactionList = transactionList;
        TransactionEditor = transactionEditor;

        NewTransactionCommand = new RelayCommand(AddNewTransaction);

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
        var row = await TransactionList.CreateNewTransactionAsync();
        SelectedTransaction = row;
    }

    private async void TransactionEditor_TransactionSaved(object? sender, string transactionId)
    {
        var refreshedRow = await TransactionList.RefreshTransactionAsync(transactionId);

        if (refreshedRow is null)
            return;

        var isStillVisible = TransactionList
            .FilteredTransactions.Cast<TransactionListRowViewModel>()
            .Any(row => row.TransactionId == transactionId);

        if (isStillVisible)
        {
            SelectedTransaction = refreshedRow;
            return;
        }

        SelectedTransaction = TransactionList
            .FilteredTransactions.Cast<TransactionListRowViewModel>()
            .FirstOrDefault();
    }
}
