namespace FarmBooks.UI.ViewModels;

public sealed class AccountingCodeOptionViewModel
{
    public string CodeId { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public string DisplayName => $"{Code} - {Name}";
}
