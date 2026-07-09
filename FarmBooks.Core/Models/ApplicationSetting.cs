namespace FarmBooks.Core.Models;

public sealed class ApplicationSetting
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public DateTime UpdatedAt { get; set; }
}