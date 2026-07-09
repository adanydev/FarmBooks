namespace FarmBooks.Core.Models;

public sealed class AuditLogEntry
{
    public string AuditLogEntryId { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Action { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
}