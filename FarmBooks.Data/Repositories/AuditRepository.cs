using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class AuditRepository
{
    private readonly DbConnectionFactory _db;

    public AuditRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(AuditLogEntry entry)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO AuditLogEntries (
            AuditLogEntryId,
            EntityType,
            EntityId,
            Action,
            Timestamp,
            OldValuesJson,
            NewValuesJson
        )
        VALUES (
            @AuditLogEntryId,
            @EntityType,
            @EntityId,
            @Action,
            @Timestamp,
            @OldValuesJson,
            @NewValuesJson
        );
        """, entry);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> ListRecentAsync(int limit = 50)
    {
        using var connection = _db.CreateConnection();

        var entries = await connection.QueryAsync<AuditLogEntry>("""
        SELECT *
        FROM AuditLogEntries
        ORDER BY Timestamp DESC
        LIMIT @Limit;
        """, new { Limit = limit });

        return entries.ToList();
    }
}