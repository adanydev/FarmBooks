namespace FarmBooks.Core.DTOs.Transactions;

public sealed class TransactionDocumentDto
{
    public string TransactionDocumentId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}