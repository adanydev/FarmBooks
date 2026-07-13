using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class TransactionDocumentRepository
{
    private readonly DbConnectionFactory _db;

    public TransactionDocumentRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(TransactionDocument document)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO TransactionDocuments (
                TransactionDocumentId,
                TransactionId,
                FileName,
                MimeType,
                DocumentBlob,
                ThumbnailBlob,
                DocumentType,
                UploadedAt,
                DeletedAt
            )
            VALUES (
                @TransactionDocumentId,
                @TransactionId,
                @FileName,
                @MimeType,
                @DocumentBlob,
                @ThumbnailBlob,
                @DocumentType,
                @UploadedAt,
                @DeletedAt
            );
            """,
            document
        );
    }

    public async Task<IReadOnlyList<TransactionDocument>> ListForTransactionAsync(
        string transactionId
    )
    {
        using var connection = _db.CreateConnection();

        var documents = await connection.QueryAsync<TransactionDocument>(
            """
            SELECT *
            FROM TransactionDocuments
            WHERE TransactionId = @TransactionId
              AND DeletedAt IS NULL
            ORDER BY UploadedAt DESC;
            """,
            new { TransactionId = transactionId }
        );

        return documents.ToList();
    }

    public async Task<TransactionDocument?> GetAsync(string transactionDocumentId)
    {
        using var connection = _db.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TransactionDocument>(
            """
            SELECT *
            FROM TransactionDocuments
            WHERE TransactionDocumentId = @TransactionDocumentId
              AND DeletedAt IS NULL;
            """,
            new { TransactionDocumentId = transactionDocumentId }
        );
    }

    public async Task SoftDeleteAsync(string transactionDocumentId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE TransactionDocuments
            SET DeletedAt = @Now
            WHERE TransactionDocumentId = @TransactionDocumentId
              AND DeletedAt IS NULL;
            """,
            new { TransactionDocumentId = transactionDocumentId, Now = DateTime.UtcNow }
        );
    }

    public async Task<int> CountForTransactionAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM TransactionDocuments
            WHERE TransactionId = @TransactionId
            AND DeletedAt IS NULL;
            """,
            new { TransactionId = transactionId }
        );
    }
}
