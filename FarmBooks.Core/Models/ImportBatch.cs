namespace FarmBooks.Core.Models;

public sealed class ImportBatch
{
    public string ImportBatchId { get; set; } = "";
    public string SourceFile { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}