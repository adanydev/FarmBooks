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
    public event EventHandler<ExpenseDetailsViewModel>? ExpenseSaved;

    public ExpenseEditorViewModel(IExpenseService expenseService)
    {
        _expenseService = expenseService;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public ExpenseDetailsViewModel? Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    public ExpenseLineItemsViewModel LineItems { get; } = new();

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public async Task LoadAsync(ExpenseListRowViewModel? row)
    {
        LineItems.LineItems.Clear();

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

        foreach (var lineItem in expense.LineItems)
        {
            LineItems.LineItems.Add(MapLineItem(lineItem));
        }

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

    private static ExpenseLineItemViewModel MapLineItem(ExpenseLineItemDto lineItem)
    {
        return new ExpenseLineItemViewModel
        {
            ExpenseLineItemId = lineItem.ExpenseLineItemId,
            CodeId = lineItem.CodeId ?? "",
            AccountingCode = lineItem.Code ?? lineItem.CodeName ?? "",
            Description = lineItem.Description ?? "",
            Amount = lineItem.Total,
        };
    }

    private async Task SaveAsync()
    {
        if (Details is null)
            return;

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

        Message = "Saved.";
        ExpenseSaved?.Invoke(this, Details);
    }
}
