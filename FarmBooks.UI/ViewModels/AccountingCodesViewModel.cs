using System.Collections.ObjectModel;
using System.Windows.Input;
using FarmBooks.Core.Models;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class AccountingCodesViewModel : ViewModelBase
{
    private readonly IAccountingCodeService _accountingCodeService;
    private AccountingCodeRowViewModel? _selectedCode;
    private string _message = "";

    public AccountingCodesViewModel(IAccountingCodeService accountingCodeService)
    {
        _accountingCodeService = accountingCodeService;

        NewCodeCommand = new RelayCommand(AddNewCode);
        SaveCommand = new AsyncRelayCommand(SaveAsync);

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
