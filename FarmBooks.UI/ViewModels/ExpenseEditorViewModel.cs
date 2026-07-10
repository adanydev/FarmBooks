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
        return new ExpenseDetailsViewModel
        {
            ExpenseId = expense.ExpenseId,
            ExpenseDate = expense.ExpenseDate,
            PaidDate = expense.PaidDate,
            SourceType = expense.SourceType,
            DocumentNumber = expense.DocumentNumber ?? "",
            BusinessName = expense.BusinessName ?? "",
            Description = expense.Description ?? "",
            Total = expense.Total,
            VATC = expense.VATC ?? 0m,
            VATS = expense.VATS ?? 0m,
            Status = expense.Status,
            Notes = expense.Notes ?? "",
        };
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
