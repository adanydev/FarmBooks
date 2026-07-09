using System.Text.Json;
using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class AuditService
{
    private readonly AuditRepository _auditRepository;

    public AuditService(AuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public Task WriteAsync(
        string entityType,
        string entityId,
        string action,
        object? oldValues = null,
        object? newValues = null)
    {
        var entry = new AuditLogEntry
        {
            AuditLogEntryId = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            OldValuesJson = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValuesJson = newValues is null ? null : JsonSerializer.Serialize(newValues)
        };

        return _auditRepository.AddAsync(entry);
    }

    public Task<IReadOnlyList<AuditLogEntry>> ListRecentAsync(int limit = 50)
    {
        return _auditRepository.ListRecentAsync(limit);
    }
}