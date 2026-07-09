using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class ImportRepository
{
    private readonly DbConnectionFactory _db;

    public ImportRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateBatchAsync(ImportBatch batch)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO ImportBatches (
            ImportBatchId,
            SourceFile,
            Status,
            Notes,
            CreatedAt,
            CompletedAt
        )
        VALUES (
            @ImportBatchId,
            @SourceFile,
            @Status,
            @Notes,
            @CreatedAt,
            @CompletedAt
        );
        """, batch);
    }

    public async Task AddRowAsync(ImportBatchRow row)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO ImportBatchRows (
            ImportBatchRowId,
            ImportBatchId,
            RowNumber,
            EntityType,
            RawJson,
            ValidationErrors,
            ImportedEntityId,
            CreatedAt
        )
        VALUES (
            @ImportBatchRowId,
            @ImportBatchId,
            @RowNumber,
            @EntityType,
            @RawJson,
            @ValidationErrors,
            @ImportedEntityId,
            @CreatedAt
        );
        """, row);
    }

    public async Task<IReadOnlyList<ImportBatch>> ListBatchesAsync()
    {
        using var connection = _db.CreateConnection();

        var batches = await connection.QueryAsync<ImportBatch>("""
        SELECT *
        FROM ImportBatches
        ORDER BY CreatedAt DESC;
        """);

        return batches.ToList();
    }

    public async Task<IReadOnlyList<ImportBatchRow>> ListRowsAsync(string importBatchId)
    {
        using var connection = _db.CreateConnection();

        var rows = await connection.QueryAsync<ImportBatchRow>("""
        SELECT *
        FROM ImportBatchRows
        WHERE ImportBatchId = @ImportBatchId
        ORDER BY RowNumber;
        """, new { ImportBatchId = importBatchId });

        return rows.ToList();
    }
}