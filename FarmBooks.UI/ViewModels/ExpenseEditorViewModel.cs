using System.ComponentModel;
using System.Windows.Input;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseEditorViewModel : ViewModelBase
{
    public ICommand SaveCommand { get; }
    private readonly IExpenseService _expenseService;
    private ExpenseDetailsViewModel? _details;
    private string _message = "";
    public event EventHandler<string>? ExpenseSaved;

    private bool _isLoading;
    private bool _hasUnsavedChanges;
    private string _validationMessage = "";
    private IReadOnlyList<ExpenseWorkflowIssueDto> _outstandingItems = [];

    public IReadOnlyList<ExpenseWorkflowIssueDto> OutstandingItems
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

    public ExpenseEditorViewModel(
        IExpenseService expenseService,
        ExpenseLineItemsViewModel lineItems
    )
    {
        _expenseService = expenseService;
        LineItems = lineItems;

        SaveCommand = new AsyncRelayCommand(SaveAsync);

        LineItems.Changed += LineItems_Changed;
    }

    private void SetWorkflowIssues(ExpenseWorkflowStatusDto workflowStatus)
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

    public ExpenseDetailsViewModel? Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    public ExpenseLineItemsViewModel LineItems { get; }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public async Task LoadAsync(ExpenseListRowViewModel? row)
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

            var expense = await _expenseService.GetExpenseDetailsAsync(row.ExpenseId);

            if (expense is null)
            {
                Details = null;
                Message = "Expense could not be loaded.";
                return;
            }

            Details = MapDetails(expense);
            Details.PropertyChanged += Details_PropertyChanged;
            SetWorkflowIssues(expense.WorkflowStatus);
            LineItems.Load(expense.LineItems);

            ValidationMessage = "";
            HasUnsavedChanges = false;
            Message = "Saved";
        }
        catch (Exception ex)
        {
            Message = $"Could not load expense: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static ExpenseDetailsViewModel MapDetails(ExpenseDetailsDto expense)
    {
        var details = new ExpenseDetailsViewModel
        {
            ExpenseId = expense.ExpenseId,
            ExpenseDate = expense.ExpenseDate,
            PaidDate = expense.PaidDate,
            SourceType = expense.SourceType,
            DocumentNumber = expense.DocumentNumber ?? "",
            BusinessName = expense.BusinessName ?? "",
            Description = expense.Description ?? "",
            Total = expense.Total,
            Notes = expense.Notes ?? "",
            Status = expense.Status,
        };

        details.LoadVatValues(
            expense.VatApplicability,
            expense.VatEntryMethod,
            expense.VATC,
            expense.VATS,
            expense.IsVatClassificationConfirmed
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

            await _expenseService.UpdateExpenseAsync(
                expenseId: Details.ExpenseId,
                expenseDate: Details.ExpenseDate,
                paidDate: Details.PaidDate,
                sourceType: Enum.Parse<ExpenseSourceType>(
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

            await LineItems.SaveAsync(Details.ExpenseId);

            var refreshedExpense = await _expenseService.GetExpenseDetailsAsync(Details.ExpenseId);

            if (refreshedExpense is not null)
            {
                _isLoading = true;

                if (Details is not null)
                {
                    Details.PropertyChanged -= Details_PropertyChanged;
                }

                Details = MapDetails(refreshedExpense);
                SetWorkflowIssues(refreshedExpense.WorkflowStatus);
                Details.PropertyChanged += Details_PropertyChanged;

                LineItems.Load(refreshedExpense.LineItems);

                _isLoading = false;
            }

            HasUnsavedChanges = false;
            Message = "Saved";

            ExpenseSaved?.Invoke(this, Details.ExpenseId);
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
