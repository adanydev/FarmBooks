using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using FarmBooks.Core.DTOs.Transactions;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class TransactionLineItemsViewModel : ViewModelBase
{
    private readonly IAccountingCodeService _accountingCodeService;
    private readonly ITransactionLineItemService _transactionLineItemService;

    private TransactionLineItemViewModel? _selectedLineItem;
    private readonly HashSet<string> _deletedLineItemIds = [];
    public event EventHandler? Changed;

    public TransactionLineItemsViewModel(
        IAccountingCodeService accountingCodeService,
        ITransactionLineItemService transactionLineItemService
    )
    {
        _accountingCodeService = accountingCodeService;
        _transactionLineItemService = transactionLineItemService;

        AddLineItemCommand = new RelayCommand(AddLineItem);
        RemoveLineItemCommand = new RelayCommand(
            RemoveSelectedLineItem,
            () => SelectedLineItem is not null
        );
        LineItems.CollectionChanged += LineItems_CollectionChanged;
        _ = LoadAccountingCodesAsync();
    }

    public ObservableCollection<TransactionLineItemViewModel> LineItems { get; } = [];

    public ObservableCollection<AccountingCodeOptionViewModel> AccountingCodes { get; } = [];

    public TransactionLineItemViewModel? SelectedLineItem
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

    public void Load(IReadOnlyList<TransactionLineItemDto> lineItems)
    {
        LineItems.Clear();
        _deletedLineItemIds.Clear();

        foreach (var item in lineItems)
        {
            LineItems.Add(
                new TransactionLineItemViewModel
                {
                    TransactionLineItemId = item.TransactionLineItemId,
                    CodeId = item.CodeId ?? "",
                    AccountingCode = item.Code ?? item.CodeName ?? "",
                    Description = item.Description ?? "",
                    Amount = item.Total,
                }
            );
        }

        SelectedLineItem = null;
    }

    public async Task SaveAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new InvalidOperationException(
                "The transaction must be saved before its line items can be saved."
            );
        }

        foreach (var deletedLineItemId in _deletedLineItemIds)
        {
            await _transactionLineItemService.DeleteLineItemAsync(deletedLineItemId);
        }

        _deletedLineItemIds.Clear();

        foreach (var lineItem in LineItems)
        {
            if (string.IsNullOrWhiteSpace(lineItem.TransactionLineItemId))
            {
                lineItem.TransactionLineItemId = await _transactionLineItemService.AddLineItemAsync(
                    transactionId,
                    NullIfWhiteSpace(lineItem.CodeId),
                    NullIfWhiteSpace(lineItem.Description),
                    lineItem.Amount
                );
            }
            else
            {
                await _transactionLineItemService.UpdateLineItemAsync(
                    lineItem.TransactionLineItemId,
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
            foreach (TransactionLineItemViewModel item in e.OldItems)
            {
                item.PropertyChanged -= LineItem_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (TransactionLineItemViewModel item in e.NewItems)
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
        var lineItem = new TransactionLineItemViewModel
        {
            TransactionLineItemId = "",
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

        if (!string.IsNullOrWhiteSpace(SelectedLineItem.TransactionLineItemId))
        {
            _deletedLineItemIds.Add(SelectedLineItem.TransactionLineItemId);
        }

        LineItems.Remove(SelectedLineItem);
        SelectedLineItem = null;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
