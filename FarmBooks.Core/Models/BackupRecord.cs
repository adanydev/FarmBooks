namespace FarmBooks.Core.Models;

public sealed class BackupRecord
{
    public string BackupRecordId { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool WasSuccessful { get; set; }
    public string? Notes { get; set; }
}