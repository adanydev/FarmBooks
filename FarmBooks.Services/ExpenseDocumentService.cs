using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class ExpenseDocumentService
{
    private readonly ExpenseDocumentRepository _documents;
    private readonly ExpenseRepository _expenses;

    public ExpenseDocumentService(
        ExpenseDocumentRepository documents,
        ExpenseRepository expenses)
    {
        _documents = documents;
        _expenses = expenses;
    }

    public async Task<string> AttachDocumentAsync(
        string expenseId,
        string filePath,
        string documentType = "Other")
    {
        var expense = await _expenses.GetAsync(expenseId);

        if (expense is null)
            throw new InvalidOperationException("Expense not found.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document file was not found.", filePath);

        var fileBytes = await File.ReadAllBytesAsync(filePath);

        var document = new ExpenseDocument
        {
            ExpenseDocumentId = Guid.NewGuid().ToString(),
            ExpenseId = expenseId,
            FileName = Path.GetFileName(filePath),
            MimeType = GetMimeType(filePath),
            DocumentBlob = fileBytes,
            ThumbnailBlob = null,
            DocumentType = documentType,
            UploadedAt = DateTime.UtcNow
        };

        await _documents.AddAsync(document);

        return document.ExpenseDocumentId;
    }

    public Task<IReadOnlyList<ExpenseDocument>> ListForExpenseAsync(string expenseId)
    {
        return _documents.ListForExpenseAsync(expenseId);
    }

    public Task<ExpenseDocument?> GetAsync(string expenseDocumentId)
    {
        return _documents.GetAsync(expenseDocumentId);
    }

    public Task DeleteAsync(string expenseDocumentId)
    {
        return _documents.SoftDeleteAsync(expenseDocumentId);
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
            _ => "application/octet-stream"
        };
    }

    public Task<int> CountForExpenseAsync(string expenseId)
    {
        return _documents.CountForExpenseAsync(expenseId);
    }
}