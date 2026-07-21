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
        MoveLineItemUpCommand = new RelayCommand(
            MoveSelectedLineItemUp,
            () => GetSelectedLineItemIndex() > 0
        );
        MoveLineItemDownCommand = new RelayCommand(
            MoveSelectedLineItemDown,
            () =>
            {
                var index = GetSelectedLineItemIndex();
                return index >= 0 && index < LineItems.Count - 1;
            }
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
                RaiseMoveCanExecuteChanged();
            }
        }
    }

    public ICommand AddLineItemCommand { get; }

    public ICommand RemoveLineItemCommand { get; }

    public ICommand MoveLineItemUpCommand { get; }

    public ICommand MoveLineItemDownCommand { get; }

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
                    StatementOrder = item.StatementOrder,
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

        for (var index = 0; index < LineItems.Count; index++)
        {
            var lineItem = LineItems[index];
            lineItem.StatementOrder = index + 1;
            lineItem.CodeId = await ResolveCodeIdAsync(lineItem);

            if (string.IsNullOrWhiteSpace(lineItem.TransactionLineItemId))
            {
                lineItem.TransactionLineItemId = await _transactionLineItemService.AddLineItemAsync(
                    transactionId,
                    NullIfWhiteSpace(lineItem.CodeId),
                    NullIfWhiteSpace(lineItem.Description),
                    lineItem.Amount,
                    lineItem.StatementOrder
                );
            }
            else
            {
                await _transactionLineItemService.UpdateLineItemAsync(
                    lineItem.TransactionLineItemId,
                    NullIfWhiteSpace(lineItem.CodeId),
                    NullIfWhiteSpace(lineItem.Description),
                    lineItem.Amount,
                    lineItem.StatementOrder
                );
            }
        }
    }

    private async Task<string> ResolveCodeIdAsync(TransactionLineItemViewModel lineItem)
    {
        var enteredCode = NullIfWhiteSpace(lineItem.AccountingCode);

        if (enteredCode is null)
        {
            return "";
        }

        var selectedCode = AccountingCodes.FirstOrDefault(code => code.CodeId == lineItem.CodeId);

        if (selectedCode is not null && MatchesEnteredCode(selectedCode, enteredCode))
        {
            return selectedCode.CodeId;
        }

        var matchingCode = AccountingCodes.FirstOrDefault(code =>
            MatchesEnteredCode(code, enteredCode)
        );

        if (matchingCode is not null)
        {
            return matchingCode.CodeId;
        }

        var codeId = await _accountingCodeService.CreateCodeAsync(enteredCode, enteredCode);

        AccountingCodes.Add(
            new AccountingCodeOptionViewModel
            {
                CodeId = codeId,
                Code = enteredCode,
                Name = enteredCode,
            }
        );

        return codeId;
    }

    private static bool MatchesEnteredCode(
        AccountingCodeOptionViewModel code,
        string enteredCode
    )
    {
        return string.Equals(code.Code, enteredCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(code.DisplayName, enteredCode, StringComparison.OrdinalIgnoreCase);
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
        RaiseMoveCanExecuteChanged();
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
            StatementOrder = LineItems.Count + 1,
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

    private void MoveSelectedLineItemUp()
    {
        var index = GetSelectedLineItemIndex();

        if (index <= 0)
            return;

        LineItems.Move(index, index - 1);
        UpdateStatementOrders();
    }

    private void MoveSelectedLineItemDown()
    {
        var index = GetSelectedLineItemIndex();

        if (index < 0 || index >= LineItems.Count - 1)
            return;

        LineItems.Move(index, index + 1);
        UpdateStatementOrders();
    }

    private int GetSelectedLineItemIndex()
    {
        return SelectedLineItem is null ? -1 : LineItems.IndexOf(SelectedLineItem);
    }

    private void UpdateStatementOrders()
    {
        for (var index = 0; index < LineItems.Count; index++)
        {
            LineItems[index].StatementOrder = index + 1;
        }

        RaiseMoveCanExecuteChanged();
    }

    private void RaiseMoveCanExecuteChanged()
    {
        ((RelayCommand)MoveLineItemUpCommand).RaiseCanExecuteChanged();
        ((RelayCommand)MoveLineItemDownCommand).RaiseCanExecuteChanged();
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
