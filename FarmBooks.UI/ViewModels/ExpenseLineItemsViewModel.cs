using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using FarmBooks.Core.DTOs.Expenses;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class ExpenseLineItemsViewModel : ViewModelBase
{
    private readonly IAccountingCodeService _accountingCodeService;
    private readonly IExpenseLineItemService _expenseLineItemService;

    private ExpenseLineItemViewModel? _selectedLineItem;
    private readonly HashSet<string> _deletedLineItemIds = [];
    public event EventHandler? Changed;

    public ExpenseLineItemsViewModel(
        IAccountingCodeService accountingCodeService,
        IExpenseLineItemService expenseLineItemService
    )
    {
        _accountingCodeService = accountingCodeService;
        _expenseLineItemService = expenseLineItemService;

        AddLineItemCommand = new RelayCommand(AddLineItem);
        RemoveLineItemCommand = new RelayCommand(
            RemoveSelectedLineItem,
            () => SelectedLineItem is not null
        );
        LineItems.CollectionChanged += LineItems_CollectionChanged;
        _ = LoadAccountingCodesAsync();
    }

    public ObservableCollection<ExpenseLineItemViewModel> LineItems { get; } = [];

    public ObservableCollection<AccountingCodeOptionViewModel> AccountingCodes { get; } = [];

    public ExpenseLineItemViewModel? SelectedLineItem
    {
        get => _selectedLineItem;
        set
        {
            if (SetProperty(ref _selectedLineItem, value))
            {
                ((RelayCommand)RemoveLineItemCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand AddLineItemCommand { get; }

    public ICommand RemoveLineItemCommand { get; }

    public void Load(IReadOnlyList<ExpenseLineItemDto> lineItems)
    {
        LineItems.Clear();
        _deletedLineItemIds.Clear();

        foreach (var item in lineItems)
        {
            LineItems.Add(
                new ExpenseLineItemViewModel
                {
                    ExpenseLineItemId = item.ExpenseLineItemId,
                    CodeId = item.CodeId ?? "",
                    AccountingCode = item.Code ?? item.CodeName ?? "",
                    Description = item.Description ?? "",
                    Amount = item.Total,
                }
            );
        }

        SelectedLineItem = null;
    }

    public async Task SaveAsync(string expenseId)
    {
        if (string.IsNullOrWhiteSpace(expenseId))
        {
            throw new InvalidOperationException(
                "The expense must be saved before its line items can be saved."
            );
        }

        foreach (var deletedLineItemId in _deletedLineItemIds)
        {
            await _expenseLineItemService.DeleteLineItemAsync(deletedLineItemId);
        }

        _deletedLineItemIds.Clear();

        foreach (var lineItem in LineItems)
        {
            if (string.IsNullOrWhiteSpace(lineItem.ExpenseLineItemId))
            {
                lineItem.ExpenseLineItemId = await _expenseLineItemService.AddLineItemAsync(
                    expenseId,
                    NullIfWhiteSpace(lineItem.CodeId),
                    NullIfWhiteSpace(lineItem.Description),
                    lineItem.Amount
                );
            }
            else
            {
                await _expenseLineItemService.UpdateLineItemAsync(
                    lineItem.ExpenseLineItemId,
                    NullIfWhiteSpace(lineItem.CodeId),
                    NullIfWhiteSpace(lineItem.Description),
                    lineItem.Amount
                );
            }
        }
    }

    private void LineItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (ExpenseLineItemViewModel item in e.OldItems)
            {
                item.PropertyChanged -= LineItem_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (ExpenseLineItemViewModel item in e.NewItems)
            {
                item.PropertyChanged += LineItem_PropertyChanged;
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void LineItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task LoadAccountingCodesAsync()
    {
        AccountingCodes.Clear();

        var codes = await _accountingCodeService.ListActiveCodesAsync();

        foreach (var code in codes)
        {
            AccountingCodes.Add(
                new AccountingCodeOptionViewModel
                {
                    CodeId = code.CodeId,
                    Code = code.Code,
                    Name = code.Name,
                }
            );
        }
    }

    private void AddLineItem()
    {
        var lineItem = new ExpenseLineItemViewModel
        {
            ExpenseLineItemId = "",
            CodeId = "",
            AccountingCode = "",
            Description = "",
            Amount = 0m,
        };

        LineItems.Add(lineItem);
        SelectedLineItem = lineItem;
    }

    private void RemoveSelectedLineItem()
    {
        if (SelectedLineItem is null)
            return;

        if (!string.IsNullOrWhiteSpace(SelectedLineItem.ExpenseLineItemId))
        {
            _deletedLineItemIds.Add(SelectedLineItem.ExpenseLineItemId);
        }

        LineItems.Remove(SelectedLineItem);
        SelectedLineItem = null;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
