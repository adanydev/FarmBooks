using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class BackupRepository
{
    private readonly DbConnectionFactory _db;

    public BackupRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(BackupRecord record)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO BackupRecords (
            BackupRecordId,
            FilePath,
            CreatedAt,
            WasSuccessful,
            Notes
        )
        VALUES (
            @BackupRecordId,
            @FilePath,
            @CreatedAt,
            @WasSuccessful,
            @Notes
        );
        """, record);
    }

    public async Task<BackupRecord?> GetLastSuccessfulAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<BackupRecord>("""
        SELECT *
        FROM BackupRecords
        WHERE WasSuccessful = 1
        ORDER BY CreatedAt DESC
        LIMIT 1;
        """);
    }

    public async Task<IReadOnlyList<BackupRecord>> ListAsync()
    {
        using var connection = _db.CreateConnection();

        var records = await connection.QueryAsync<BackupRecord>("""
        SELECT *
        FROM BackupRecords
        ORDER BY CreatedAt DESC;
        """);

        return records.ToList();
    }
}