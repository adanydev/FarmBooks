namespace FarmBooks.Core.Models;

public sealed class ImportBatchRow
{
    public string ImportBatchRowId { get; set; } = "";
    public string ImportBatchId { get; set; } = "";
    public int RowNumber { get; set; }
    public string EntityType { get; set; } = "";
    public string RawJson { get; set; } = "";
    public string? ValidationErrors { get; set; }
    public string? ImportedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}