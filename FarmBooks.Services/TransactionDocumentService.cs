using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class TransactionDocumentService
{
    private readonly TransactionDocumentRepository _documents;
    private readonly TransactionRepository _transactions;

    public TransactionDocumentService(
        TransactionDocumentRepository documents,
        TransactionRepository transactions
    )
    {
        _documents = documents;
        _transactions = transactions;
    }

    public async Task<string> AttachDocumentAsync(
        string transactionId,
        string filePath,
        string documentType = "Other"
    )
    {
        var transaction = await _transactions.GetAsync(transactionId);

        if (transaction is null)
            throw new InvalidOperationException("Transaction not found.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document file was not found.", filePath);

        var fileBytes = await File.ReadAllBytesAsync(filePath);

        var document = new TransactionDocument
        {
            TransactionDocumentId = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            FileName = Path.GetFileName(filePath),
            MimeType = GetMimeType(filePath),
            DocumentBlob = fileBytes,
            ThumbnailBlob = null,
            DocumentType = documentType,
            UploadedAt = DateTime.UtcNow,
        };

        await _documents.AddAsync(document);

        return document.TransactionDocumentId;
    }

    public Task<IReadOnlyList<TransactionDocument>> ListForTransactionAsync(string transactionId)
    {
        return _documents.ListForTransactionAsync(transactionId);
    }

    public Task<TransactionDocument?> GetAsync(string transactionDocumentId)
    {
        return _documents.GetAsync(transactionDocumentId);
    }

    public Task DeleteAsync(string transactionDocumentId)
    {
        return _documents.SoftDeleteAsync(transactionDocumentId);
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            _ => "application/octet-stream",
        };
    }

    public Task<int> CountForTransactionAsync(string transactionId)
    {
        return _documents.CountForTransactionAsync(transactionId);
    }
}
