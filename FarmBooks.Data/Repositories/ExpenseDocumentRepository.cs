using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class ExpenseDocumentRepository
{
    private readonly DbConnectionFactory _db;

    public ExpenseDocumentRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(ExpenseDocument document)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO ExpenseDocuments (
                ExpenseDocumentId,
                ExpenseId,
                FileName,
                MimeType,
                DocumentBlob,
                ThumbnailBlob,
                DocumentType,
                UploadedAt,
                DeletedAt
            )
            VALUES (
                @ExpenseDocumentId,
                @ExpenseId,
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

    public async Task<IReadOnlyList<ExpenseDocument>> ListForExpenseAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        var documents = await connection.QueryAsync<ExpenseDocument>(
            """
            SELECT *
            FROM ExpenseDocuments
            WHERE ExpenseId = @ExpenseId
              AND DeletedAt IS NULL
            ORDER BY UploadedAt DESC;
            """,
            new { ExpenseId = expenseId }
        );

        return documents.ToList();
    }

    public async Task<ExpenseDocument?> GetAsync(string expenseDocumentId)
    {
        using var connection = _db.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<ExpenseDocument>(
            """
            SELECT *
            FROM ExpenseDocuments
            WHERE ExpenseDocumentId = @ExpenseDocumentId
              AND DeletedAt IS NULL;
            """,
            new { ExpenseDocumentId = expenseDocumentId }
        );
    }

    public async Task SoftDeleteAsync(string expenseDocumentId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE ExpenseDocuments
            SET DeletedAt = @Now
            WHERE ExpenseDocumentId = @ExpenseDocumentId
              AND DeletedAt IS NULL;
            """,
            new { ExpenseDocumentId = expenseDocumentId, Now = DateTime.UtcNow }
        );
    }

    public async Task<int> CountForExpenseAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM ExpenseDocuments
            WHERE ExpenseId = @ExpenseId
            AND DeletedAt IS NULL;
            """,
            new { ExpenseId = expenseId }
        );
    }
}
