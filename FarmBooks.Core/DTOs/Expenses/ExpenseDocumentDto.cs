namespace FarmBooks.Core.DTOs.Expenses;

public sealed class ExpenseDocumentDto
{
    public string ExpenseDocumentId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}