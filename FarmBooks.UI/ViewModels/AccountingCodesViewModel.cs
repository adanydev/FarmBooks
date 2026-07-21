using System.Collections.ObjectModel;
using System.Windows.Input;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;
using FarmBooks.UI.Services;

namespace FarmBooks.UI.ViewModels;

public sealed class AccountingCodesViewModel : ViewModelBase
{
    private readonly IAccountingCodeService _accountingCodeService;
    private readonly IConfirmationService _confirmationService;
    private AccountingCodeRowViewModel? _selectedCode;
    private string _message = "";

    public AccountingCodesViewModel(
        IAccountingCodeService accountingCodeService,
        IConfirmationService confirmationService
    )
    {
        _accountingCodeService = accountingCodeService;
        _confirmationService = confirmationService;

        NewCodeCommand = new RelayCommand(AddNewCode);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);

        _ = LoadAsync();
    }

    public ObservableCollection<AccountingCodeRowViewModel> Codes { get; } = new();

    public AccountingCodeRowViewModel? SelectedCode
    {
        get => _selectedCode;
        set => SetProperty(ref _selectedCode, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public ICommand NewCodeCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    private async Task LoadAsync()
    {
        Codes.Clear();

        var codes = await _accountingCodeService.ListAllCodesAsync();

        foreach (var code in codes)
        {
            Codes.Add(MapToRow(code));
        }

        SelectedCode = Codes.FirstOrDefault();
    }

    private void AddNewCode()
    {
        var row = new AccountingCodeRowViewModel
        {
            CodeId = "",
            Code = "",
            Name = "",
            Description = "",
            IsActive = true,
        };

        Codes.Insert(0, row);
        SelectedCode = row;
    }

    private async Task SaveAsync()
    {
        if (SelectedCode is null)
            return;

        if (string.IsNullOrWhiteSpace(SelectedCode.Code))
        {
            Message = "Code is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedCode.Name))
        {
            Message = "Name is required.";
            return;
        }

        Message = "Saving...";

        if (string.IsNullOrWhiteSpace(SelectedCode.CodeId))
        {
            var codeId = await _accountingCodeService.CreateCodeAsync(
                SelectedCode.Code.Trim(),
                SelectedCode.Name.Trim(),
                SelectedCode.Description.Trim()
            );

            SelectedCode.CodeId = codeId;
        }
        else
        {
            await _accountingCodeService.UpdateCodeAsync(
                SelectedCode.CodeId,
                SelectedCode.Code.Trim(),
                SelectedCode.Name.Trim(),
                SelectedCode.Description.Trim(),
                SelectedCode.IsActive
            );
        }

        Message = "Saved.";
    }

    private async Task DeleteAsync()
    {
        var code = SelectedCode;

        if (code is null)
            return;

        if (!string.IsNullOrWhiteSpace(code.CodeId))
        {
            var confirmed = _confirmationService.Confirm(
                $"Delete accounting code '{code.Code}'? Existing line items will be left uncoded.",
                "Delete Accounting Code"
            );

            if (!confirmed)
                return;

            Message = "Deleting...";
            await _accountingCodeService.DeleteCodeAsync(code.CodeId);
        }

        var index = Codes.IndexOf(code);
        Codes.Remove(code);
        SelectedCode = Codes.Count == 0 ? null : Codes[Math.Min(index, Codes.Count - 1)];
        Message = "Deleted.";
    }

    private static AccountingCodeRowViewModel MapToRow(AccountingCode code)
    {
        return new AccountingCodeRowViewModel
        {
            CodeId = code.CodeId,
            Code = code.Code,
            Name = code.Name,
            Description = code.Description ?? "",
            IsActive = code.IsActive,
        };
    }
}
