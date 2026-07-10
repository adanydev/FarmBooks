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

    public ExpenseEditorViewModel(
        IExpenseService expenseService,
        ExpenseLineItemsViewModel lineItems
    )
    {
        _expenseService = expenseService;
        LineItems = lineItems;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
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
        LineItems.Load([]);

        if (row is null)
        {
            Details = null;
            Message = "";
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

        LineItems.Load(expense.LineItems);

        Message = "";
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
        };
    }

    private async Task SaveAsync()
    {
        if (Details is null)
            return;

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
                notes: null
            );

            await LineItems.SaveAsync(Details.ExpenseId);

            var refreshedExpense = await _expenseService.GetExpenseDetailsAsync(Details.ExpenseId);

            if (refreshedExpense is not null)
            {
                Details = MapDetails(refreshedExpense);
                LineItems.Load(refreshedExpense.LineItems);
            }

            Message = "Saved.";

            ExpenseSaved?.Invoke(this, Details.ExpenseId);
        }
        catch (Exception ex)
        {
            Message = $"Could not save: {ex.Message}";
        }
    }
}
