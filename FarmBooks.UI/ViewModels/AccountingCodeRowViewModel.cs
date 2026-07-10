using FarmBooks.UI.Infrastructure;

namespace FarmBooks.UI.ViewModels;

public sealed class AccountingCodeRowViewModel : ViewModelBase
{
    private string _codeId = "";
    private string _code = "";
    private string _name = "";
    private string _description = "";
    private bool _isActive;

    public string CodeId
    {
        get => _codeId;
        set => SetProperty(ref _codeId, value);
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}
