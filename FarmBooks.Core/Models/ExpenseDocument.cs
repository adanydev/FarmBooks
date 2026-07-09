namespace FarmBooks.Core.Models;

public sealed class ExpenseDocument
{
    public string ExpenseDocumentId { get; set; } = "";
    public string ExpenseId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public byte[] DocumentBlob { get; set; } = [];
    public byte[]? ThumbnailBlob { get; set; }
    public string DocumentType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}