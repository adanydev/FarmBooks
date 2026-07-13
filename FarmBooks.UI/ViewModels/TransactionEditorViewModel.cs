using System.ComponentModel;
using System.Windows.Input;
using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionEditorViewModel : ViewModelBase
{
    public ICommand SaveCommand { get; }
    private readonly ITransactionService _transactionService;
    private TransactionDetailsViewModel? _details;
    private string _message = "";
    public event EventHandler<string>? TransactionSaved;

    private bool _isLoading;
    private bool _hasUnsavedChanges;
    private string _validationMessage = "";
    private IReadOnlyList<TransactionWorkflowIssueDto> _outstandingItems = [];

    public IReadOnlyList<TransactionWorkflowIssueDto> OutstandingItems
    {
        get => _outstandingItems;
        private set
        {
            if (SetProperty(ref _outstandingItems, value))
            {
                OnPropertyChanged(nameof(HasOutstandingItems));
                OnPropertyChanged(nameof(OutstandingItemsToolTip));
            }
        }
    }

    public bool HasOutstandingItems => OutstandingItems.Count > 0;

    public string OutstandingItemsToolTip =>
        OutstandingItems.Count == 0
            ? "No outstanding items."
            : string.Join(
                Environment.NewLine,
                OutstandingItems.Select(issue => $"• {issue.Message}")
            );

    public TransactionEditorViewModel(
        ITransactionService transactionService,
        TransactionLineItemsViewModel lineItems
    )
    {
        _transactionService = transactionService;
        LineItems = lineItems;

        SaveCommand = new AsyncRelayCommand(SaveAsync);

        LineItems.Changed += LineItems_Changed;
    }

    private void SetWorkflowIssues(TransactionWorkflowStatusDto workflowStatus)
    {
        OutstandingItems = [.. workflowStatus.VatIssues, .. workflowStatus.TaxIssues];
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public TransactionDetailsViewModel? Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    public TransactionLineItemsViewModel LineItems { get; }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public async Task LoadAsync(TransactionListRowViewModel? row)
    {
        _isLoading = true;

        try
        {
            if (Details is not null)
            {
                Details.PropertyChanged -= Details_PropertyChanged;
            }

            LineItems.Load([]);

            if (row is null)
            {
                Details = null;
                Message = "";
                ValidationMessage = "";
                HasUnsavedChanges = false;
                return;
            }

            Message = "Loading...";

            var transaction = await _transactionService.GetTransactionDetailsAsync(
                row.TransactionId
            );

            if (transaction is null)
            {
                Details = null;
                Message = "Transaction could not be loaded.";
                return;
            }

            Details = MapDetails(transaction);
            Details.PropertyChanged += Details_PropertyChanged;
            SetWorkflowIssues(transaction.WorkflowStatus);
            LineItems.Load(transaction.LineItems);

            ValidationMessage = "";
            HasUnsavedChanges = false;
            Message = "Saved";
        }
        catch (Exception ex)
        {
            Message = $"Could not load transaction: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static TransactionDetailsViewModel MapDetails(TransactionDetailsDto transaction)
    {
        var details = new TransactionDetailsViewModel
        {
            TransactionId = transaction.TransactionId,
            TransactionDate = transaction.TransactionDate,
            PaidDate = transaction.PaidDate,
            SourceType = transaction.SourceType,
            DocumentNumber = transaction.DocumentNumber ?? "",
            BusinessName = transaction.BusinessName ?? "",
            Description = transaction.Description ?? "",
            Total = transaction.Total,
            Notes = transaction.Notes ?? "",
            Status = transaction.Status,
        };

        details.LoadVatValues(
            transaction.VatApplicability,
            transaction.VatEntryMethod,
            transaction.VATC,
            transaction.VATS,
            transaction.IsVatClassificationConfirmed
        );

        return details;
    }

    private async Task SaveAsync()
    {
        if (Details is null)
            return;

        ValidationMessage = "";

        if (Details.HasErrors)
        {
            ValidationMessage = "Please correct the highlighted fields before saving.";

            Message = "Not saved";
            return;
        }

        try
        {
            Message = "Saving...";

            await _transactionService.UpdateTransactionAsync(
                transactionId: Details.TransactionId,
                transactionDate: Details.TransactionDate,
                paidDate: Details.PaidDate,
                sourceType: Enum.Parse<TransactionSourceType>(
                    Details.SourceType.Replace(" ", ""),
                    ignoreCase: true
                ),
                documentNumber: Details.DocumentNumber,
                businessName: Details.BusinessName,
                description: Details.Description,
                total: Details.Total,
                vatApplicability: Details.VatApplicability,
                vatEntryMethod: Details.VatEntryMethod,
                vatC: Details.VATC,
                vatS: Details.VATS,
                isVatClassificationConfirmed: Details.IsVatClassificationConfirmed,
                notes: Details.Notes
            );

            await LineItems.SaveAsync(Details.TransactionId);

            var refreshedTransaction = await _transactionService.GetTransactionDetailsAsync(
                Details.TransactionId
            );

            if (refreshedTransaction is not null)
            {
                _isLoading = true;

                if (Details is not null)
                {
                    Details.PropertyChanged -= Details_PropertyChanged;
                }

                Details = MapDetails(refreshedTransaction);
                SetWorkflowIssues(refreshedTransaction.WorkflowStatus);
                Details.PropertyChanged += Details_PropertyChanged;

                LineItems.Load(refreshedTransaction.LineItems);

                _isLoading = false;
            }

            HasUnsavedChanges = false;
            Message = "Saved";

            TransactionSaved?.Invoke(this, Details.TransactionId);
        }
        catch (Exception ex)
        {
            Message = "Could not save";
            ValidationMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void LineItems_Changed(object? sender, EventArgs e)
    {
        MarkAsChanged();
    }

    private void Details_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkAsChanged();
    }

    private void MarkAsChanged()
    {
        if (_isLoading)
            return;

        HasUnsavedChanges = true;
        Message = "Unsaved changes";
    }
}
